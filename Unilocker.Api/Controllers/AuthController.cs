using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Services;
using BCrypt.Net;

namespace Unilocker.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly EmailService _emailService; // ← NUEVO
    private readonly VerificationCodeService _verificationCodeService; // ← NUEVO

    public AuthController(
        UnilockerDbContext context,
        JwtService jwtService,
        ILogger<AuthController> logger,
        EmailService emailService, // ← NUEVO
        VerificationCodeService verificationCodeService) // ← NUEVO
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
        _emailService = emailService; // ← NUEVO
        _verificationCodeService = verificationCodeService; // ← NUEVO
    }

    /// <summary>
    /// Endpoint de login - Autentica usuario y envía código 2FA
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("===== INICIO LOGIN =====");
            _logger.LogInformation("Usuario recibido: {Username}", request.Username);

            // 1. Buscar usuario con su rol
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                _logger.LogWarning("❌ Usuario NO encontrado en BD");
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            _logger.LogInformation("✅ Usuario encontrado: {Username} (ID: {Id})", user.Username, user.Id);
            _logger.LogInformation("📋 RoleId del usuario: {RoleId}", user.RoleId);
            _logger.LogInformation("📋 Role cargado: {RoleLoaded}, Nombre: {RoleName}", user.Role != null, user.Role?.Name ?? "NULL");

            // 2. Verificar si el usuario está bloqueado
            if (user.IsBlocked == true)
            {
                _logger.LogWarning("❌ Usuario bloqueado");
                return StatusCode(403, new { message = "Usuario bloqueado. Contacte al administrador." });
            }

            // 3. Verificar si el usuario está inactivo
            if (!user.Status)
            {
                _logger.LogWarning("❌ Usuario inactivo");
                return StatusCode(403, new { message = "Usuario inactivo. Contacte al administrador." });
            }

            // 4. Verificar contraseña con BCrypt
            _logger.LogInformation("🔐 Verificando contraseña...");
            _logger.LogInformation("🔐 Hash almacenado: {Hash}", user.PasswordHash);
            _logger.LogInformation("🔐 Contraseña ingresada: {Password}", request.Password);

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                _logger.LogInformation("🔐 Resultado de BCrypt.Verify: {Result}", isPasswordValid);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error al verificar BCrypt: {Error}", ex.Message);
                _logger.LogError("❌ StackTrace: {StackTrace}", ex.StackTrace);
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            if (!isPasswordValid)
            {
                _logger.LogWarning("❌ Contraseña incorrecta");

                // Incrementar intentos fallidos
                user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                // Bloquear si excede 5 intentos
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsBlocked = true;
                    _logger.LogWarning("🔒 Usuario bloqueado por múltiples intentos");
                }

                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            _logger.LogInformation("✅ Contraseña válida");

            // 5. Verificar si el usuario tiene rol de Administrador (solo Admin puede acceder a la web)
            _logger.LogInformation("🔍 Verificando rol del usuario...");
            _logger.LogInformation("📋 user.Role es null: {IsNull}", user.Role == null);
            
            if (user.Role != null)
            {
                _logger.LogInformation("📋 Nombre del rol: '{RoleName}'", user.Role.Name);
                _logger.LogInformation("📋 Comparación con 'Administrador': {IsAdmin}", user.Role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase));
            }

            if (user.Role == null || !user.Role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("❌ Usuario sin permisos de administrador - Rol: {RoleName}, RoleId: {RoleId}", 
                    user.Role?.Name ?? "Sin rol", user.RoleId);
                return StatusCode(403, new { message = "Acceso denegado. Solo usuarios con rol de Administrador pueden acceder a la plataforma web." });
            }

            _logger.LogInformation("✅ Usuario tiene rol de Administrador");

            // 6. Verificar si tiene email configurado
            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("⚠️ Usuario sin email - Login sin 2FA (fallback)");

                // Login sin 2FA (fallback)
                var tokenFallback = _jwtService.GenerateToken(user);
                var expiresAtFallback = _jwtService.GetTokenExpirationTime();

                user.LastAccess = DateTime.Now;
                user.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();

                return Ok(new LoginResponse
                {
                    Token = tokenFallback,
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = $"{user.FirstName} {user.LastName}",
                    RoleName = user.Role.Name,
                    RoleId = user.RoleId,
                    ExpiresAt = expiresAtFallback,
                    RequiresVerification = false
                });
            }

            // 7. Generar código de verificación 2FA
            var code = _verificationCodeService.GenerateCode();
            _verificationCodeService.SaveCode(user.Id, code);

            _logger.LogInformation("🔑 Código 2FA generado: {Code} (UserId: {UserId})", code, user.Id);

            // 8. Enviar código por email
            var emailSent = await _emailService.SendVerificationCodeAsync(user.Email, code);

            if (!emailSent)
            {
                _logger.LogError("❌ Error al enviar email - Login sin 2FA (fallback)");

                // Si falla el email, permitir login sin 2FA
                var tokenFallback = _jwtService.GenerateToken(user);
                var expiresAtFallback = _jwtService.GetTokenExpirationTime();

                user.LastAccess = DateTime.Now;
                user.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();

                return Ok(new LoginResponse
                {
                    Token = tokenFallback,
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = $"{user.FirstName} {user.LastName}",
                    RoleName = user.Role.Name,
                    RoleId = user.RoleId,
                    ExpiresAt = expiresAtFallback,
                    RequiresVerification = false
                });
            }

            _logger.LogInformation("✅ Código 2FA enviado a: {Email}", user.Email);

            // 9. Retornar respuesta indicando que se requiere verificación
            var maskedEmail = _emailService.MaskEmail(user.Email);

            return Ok(new LoginResponse
            {
                Token = null,
                UserId = user.Id,
                Username = user.Username,
                FullName = null,
                RoleName = null,
                RoleId = null,
                ExpiresAt = null,
                RequiresVerification = true,
                MaskedEmail = maskedEmail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR GENERAL en login");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
    /// <summary>
    /// Endpoint para verificar código 2FA
    /// </summary>
    [HttpPost("verify-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        try
        {
            _logger.LogInformation("===== VERIFICANDO CÓDIGO 2FA =====");
            _logger.LogInformation("UserId: {UserId}, Código: {Code}", request.UserId, request.Code);

            // 1. Validar el código
            var (isValid, message) = _verificationCodeService.ValidateCode(request.UserId, request.Code);

            if (!isValid)
            {
                _logger.LogWarning("❌ Código inválido: {Message}", message);
                return Unauthorized(new { message });
            }

            _logger.LogInformation("✅ Código válido - Generando token...");

            // 2. Buscar usuario con su rol
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
            {
                _logger.LogError("❌ Usuario no encontrado: {UserId}", request.UserId);
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            // 3. Verificar que el usuario tenga rol de Administrador
            if (user.Role == null || !user.Role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("❌ Usuario sin permisos de administrador - Rol: {RoleName}", user.Role?.Name ?? "Sin rol");
                return StatusCode(403, new { message = "Acceso denegado. Solo usuarios con rol de Administrador pueden acceder a la plataforma web." });
            }

            // 4. Generar token JWT
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpirationTime();

            // 5. Actualizar información del usuario
            user.LastAccess = DateTime.Now;
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ LOGIN 2FA EXITOSO - Usuario: {Username}", user.Username);

            // 6. Retornar respuesta completa con token
            var response = new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                FullName = $"{user.FirstName} {user.LastName}",
                RoleName = user.Role.Name,
                RoleId = user.RoleId,
                ExpiresAt = expiresAt,
                RequiresVerification = false
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR en verify-code");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }


    /// <summary>
    /// Endpoint de prueba para verificar autenticación
    /// </summary>
    [HttpGet("verify")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult VerifyToken()
    {
        var userId = User.FindFirst("userId")?.Value;
        var username = User.FindFirst("sub")?.Value;
        var roleName = User.FindFirst("roleName")?.Value;

        return Ok(new
        {
            message = "Token válido",
            userId,
            username,
            roleName
        });
    }
}