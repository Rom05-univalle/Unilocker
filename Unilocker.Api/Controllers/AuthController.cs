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

    public AuthController(
        UnilockerDbContext context,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de login - Autentica usuario y retorna JWT token
    /// </summary>
    /// <param name="request">Credenciales de usuario</param>
    /// <returns>Token JWT y datos del usuario</returns>
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
            _logger.LogInformation("Password recibido: {Password}", request.Password);

            // 1. Buscar usuario con su rol
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                _logger.LogWarning("❌ Usuario NO encontrado en BD");
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            _logger.LogInformation("✅ Usuario encontrado en BD:");
            _logger.LogInformation("  - Id: {Id}", user.Id);
            _logger.LogInformation("  - Username: {Username}", user.Username);
            _logger.LogInformation("  - PasswordHash: {Hash}", user.PasswordHash);
            _logger.LogInformation("  - Status: {Status}", user.Status);
            _logger.LogInformation("  - IsBlocked: {IsBlocked}", user.IsBlocked);
            _logger.LogInformation("  - Role cargado: {HasRole}", user.Role != null);

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
            _logger.LogInformation("  - Password input: {Input}", request.Password);
            _logger.LogInformation("  - Hash en BD: {Hash}", user.PasswordHash);

            bool isPasswordValid = false;

            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                _logger.LogInformation("  - Resultado BCrypt.Verify: {Result}", isPasswordValid);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error al verificar BCrypt: {Error}", ex.Message);
                return StatusCode(500, new { message = "Error al verificar contraseña", error = ex.Message });
            }

            if (!isPasswordValid)
            {
                _logger.LogWarning("❌ Contraseña incorrecta");

                // Incrementar intentos fallidos
                user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
                _logger.LogInformation("Intentos fallidos: {Attempts}", user.FailedLoginAttempts);

                // Bloquear si excede 5 intentos
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsBlocked = true;
                    _logger.LogWarning("🔒 Usuario bloqueado por múltiples intentos");
                }

                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            _logger.LogInformation("✅ Contraseña válida - Generando token...");

            // 5. Login exitoso - Generar token JWT
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpirationTime();

            // 6. Actualizar información del usuario
            user.LastAccess = DateTime.Now;
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ LOGIN EXITOSO");

            // 7. Retornar respuesta
            var response = new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                FullName = $"{user.FirstName} {user.LastName}",
                RoleName = user.Role.Name,
                RoleId = user.RoleId,
                ExpiresAt = expiresAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR GENERAL en login");
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