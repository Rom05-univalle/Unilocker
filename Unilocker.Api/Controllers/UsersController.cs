using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;
using Unilocker.Api.Helpers;
using Unilocker.Api.Services;
using Unilocker.Api.Extensions;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly PasswordGeneratorService _passwordGenerator;
    private readonly EmailService _emailService;

    public UsersController(
        UnilockerDbContext context, 
        ILogger<UsersController> logger,
        PasswordGeneratorService passwordGenerator,
        EmailService emailService)
    {
        _context = context;
        _logger = logger;
        _passwordGenerator = passwordGenerator;
        _emailService = emailService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Where(u => u.Status == true)
                .OrderBy(u => u.Username)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.FirstName,
                    u.LastName,
                    u.SecondLastName,
                    FullName = u.FirstName + " " + u.LastName + (u.SecondLastName != null ? " " + u.SecondLastName : ""),
                    u.RoleId,
                    RoleName = _context.Roles.Where(r => r.Id == u.RoleId).Select(r => r.Name).FirstOrDefault(),
                    IsActive = u.Status,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
        }
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.FirstName,
                    u.LastName,
                    u.SecondLastName,
                    FullName = u.FirstName + " " + u.LastName + (u.SecondLastName != null ? " " + u.SecondLastName : ""),
                    u.RoleId,
                    RoleName = _context.Roles.Where(r => r.Id == u.RoleId).Select(r => r.Name).FirstOrDefault(),
                    IsActive = u.Status,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario");
            return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
        }
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("CreateUser - Payload: {Payload}", dto.ToString());

            // Extraer valores RAW
            var usernameRaw = dto.TryGetProperty("username", out var usernameEl) ? usernameEl.GetString() : null;
            var emailRaw = dto.TryGetProperty("email", out var emailEl) && emailEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? emailEl.GetString() : null;
            var phoneRaw = dto.TryGetProperty("phone", out var phoneEl) && phoneEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? phoneEl.GetString() : null;
            var firstNameRaw = dto.TryGetProperty("firstName", out var firstNameEl) ? firstNameEl.GetString() : null;
            var lastNameRaw = dto.TryGetProperty("lastName", out var lastNameEl) ? lastNameEl.GetString() : null;
            var secondLastNameRaw = dto.TryGetProperty("secondLastName", out var secondLastNameEl) && secondLastNameEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? secondLastNameEl.GetString() : null;
            var roleId = dto.TryGetProperty("roleId", out var roleEl) ? roleEl.GetInt32() : 0;
            var status = dto.TryGetProperty("status", out var statusEl) ? statusEl.GetBoolean() : true;

            // NORMALIZAR todos los campos de texto
            var username = StringNormalizer.NormalizeUsername(usernameRaw);
            var email = StringNormalizer.NormalizeEmail(emailRaw);
            var phone = StringNormalizer.NormalizePhone(phoneRaw);
            var firstName = StringNormalizer.Normalize(firstNameRaw);
            var lastName = StringNormalizer.Normalize(lastNameRaw);
            var secondLastName = StringNormalizer.Normalize(secondLastNameRaw);

            // Validaciones DESPUÉS de normalizar
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "El username es obligatorio" });
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "El email es obligatorio" });
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest(new { message = "El apellido es obligatorio" });
            }
            if (roleId == 0)
            {
                return BadRequest(new { message = "RoleId es obligatorio" });
            }

            // GENERAR CONTRASEÑA AUTOMÁTICAMENTE
            var generatedPassword = _passwordGenerator.GeneratePassword(username!, firstName!, lastName!, secondLastName);
            _logger.LogInformation("Contraseña generada para usuario: {Username}", username);

            // Obtener usuario actual para auditoría
            var currentUserId = this.GetCurrentUserId();

            var user = new User
            {
                Username = username!,
                Email = email,
                Phone = phone,
                FirstName = firstName!,
                LastName = lastName!,
                SecondLastName = secondLastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword, 12),
                RoleId = roleId,
                Status = status,
                CreatedAt = DateTime.Now,
                CreatedUpdatedBy = currentUserId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario creado exitosamente: {UserId}", user.Id);

            // ENVIAR CONTRASEÑA POR EMAIL
            var fullName = $"{firstName} {lastName}" + (!string.IsNullOrWhiteSpace(secondLastName) ? $" {secondLastName}" : "");
            var emailSent = await _emailService.SendPasswordAsync(email!, username!, fullName, generatedPassword);

            if (!emailSent)
            {
                _logger.LogWarning("No se pudo enviar el email con la contraseña al usuario: {Email}", email);
                // No fallar la creación del usuario, pero avisar
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.RoleId,
                    warning = "Usuario creado pero no se pudo enviar el email. Contacte al administrador."
                });
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.RoleId,
                message = "Usuario creado exitosamente. La contraseña ha sido enviada por correo electrónico."
            });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear usuario");
            
            // Detectar errores de duplicación en índices filtrados
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            
            if (errorMessage.Contains("UQ_User_Username_Active", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Username", StringComparison.OrdinalIgnoreCase) && 
                (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || 
                 errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Error: Ya existe un usuario activo con ese nombre de usuario. Por favor elige otro." });
            }
            
            if (errorMessage.Contains("UQ_User_Email_Active", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase) && 
                (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || 
                 errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Error: Ya existe un usuario activo con ese correo electrónico. Por favor usa otro." });
            }
            
            // Error genérico de duplicación
            if (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Error: Ya existe un usuario activo con estas credenciales (usuario o email)." });
            }
            
            return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message });
        }
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("UpdateUser - ID: {Id}, Payload: {Payload}", id, dto.ToString());

            var existingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Obtener el usuario autenticado
            var currentUserIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "No se pudo identificar al usuario actual" });
            }

            // Validación adicional: Si se intenta cambiar el rol de un Admin, validar
            int? newRoleId = null;
            if (dto.TryGetProperty("roleId", out var roleIdEl))
            {
                newRoleId = roleIdEl.GetInt32();
                if (existingUser.Role != null && existingUser.Role.Name == "Administrador")
                {
                    // Verificar si el nuevo rol es diferente al actual
                    if (newRoleId != existingUser.RoleId)
                    {
                        // Contar cuántos admins hay actualmente (usando ToListAsync para evaluación en cliente)
                        var allUsers = await _context.Users
                            .Include(u => u.Role)
                            .Where(u => u.Status == true && u.Role != null)
                            .ToListAsync();
                        
                        var adminCount = allUsers.Count(u => u.Role.Name == "Administrador");

                        if (adminCount <= 1)
                        {
                            return BadRequest(new { message = "No puedes cambiar el rol del único administrador del sistema" });
                        }
                        else
                        {
                            return BadRequest(new { message = "No puedes modificar el rol de un usuario administrador" });
                        }
                    }
                }
            }

            if (dto.TryGetProperty("username", out var usernameEl) && usernameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var normalizedUsername = StringNormalizer.NormalizeUsername(usernameEl.GetString());
                if (!string.IsNullOrWhiteSpace(normalizedUsername))
                    existingUser.Username = normalizedUsername;
            }
            if (dto.TryGetProperty("email", out var emailEl))
            {
                existingUser.Email = StringNormalizer.NormalizeEmail(
                    emailEl.ValueKind != System.Text.Json.JsonValueKind.Null ? emailEl.GetString() : null);
            }
            if (dto.TryGetProperty("phone", out var phoneEl))
            {
                existingUser.Phone = StringNormalizer.NormalizePhone(
                    phoneEl.ValueKind != System.Text.Json.JsonValueKind.Null ? phoneEl.GetString() : null);
            }
            if (dto.TryGetProperty("firstName", out var firstNameEl) && firstNameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var normalizedFirstName = StringNormalizer.Normalize(firstNameEl.GetString());
                if (!string.IsNullOrWhiteSpace(normalizedFirstName))
                    existingUser.FirstName = normalizedFirstName;
            }
            if (dto.TryGetProperty("lastName", out var lastNameEl) && lastNameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var normalizedLastName = StringNormalizer.Normalize(lastNameEl.GetString());
                if (!string.IsNullOrWhiteSpace(normalizedLastName))
                    existingUser.LastName = normalizedLastName;
            }
            if (dto.TryGetProperty("secondLastName", out var secondLastNameEl))
            {
                existingUser.SecondLastName = StringNormalizer.Normalize(
                    secondLastNameEl.ValueKind != System.Text.Json.JsonValueKind.Null ? secondLastNameEl.GetString() : null);
            }
            if (newRoleId.HasValue && newRoleId.Value != existingUser.RoleId)
            {
                _logger.LogInformation("Cambiando rol del usuario {UserId} de RoleId {OldRoleId} a {NewRoleId}", 
                    existingUser.Id, existingUser.RoleId, newRoleId.Value);
                
                // Solo actualizar el RoleId, EF Core manejará la navegación automáticamente
                existingUser.RoleId = newRoleId.Value;
            }
            if (dto.TryGetProperty("status", out var statusEl))
            {
                existingUser.Status = statusEl.GetBoolean();
            }
            
            // Solo actualizar contraseña si se proporciona una nueva
            if (dto.TryGetProperty("passwordHash", out var passwordEl) && 
                passwordEl.ValueKind == System.Text.Json.JsonValueKind.String && 
                !string.IsNullOrEmpty(passwordEl.GetString()))
            {
                var newPassword = passwordEl.GetString();
                if (newPassword != null && newPassword.Length < 6)
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
                }
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
            }
            
            existingUser.UpdatedAt = DateTime.Now;
            existingUser.CreatedUpdatedBy = currentUserId;

            _logger.LogInformation("Guardando cambios en base de datos para usuario {UserId}", existingUser.Id);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SaveChangesAsync completado exitosamente");

            _logger.LogInformation("Usuario actualizado exitosamente: {UserId} por usuario {CurrentUserId}", id, currentUserId);
            return Ok(new
            {
                existingUser.Id,
                existingUser.Username,
                existingUser.Email,
                existingUser.FirstName,
                existingUser.LastName,
                existingUser.SecondLastName,
                existingUser.RoleId,
                existingUser.Status
            });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogError(ex, "DbUpdateException al actualizar usuario {UserId}. InnerException: {InnerException}", 
                id, ex.InnerException?.Message ?? "null");
            
            // Detectar errores de duplicación en índices filtrados
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            
            if (errorMessage.Contains("UQ_User_Username_Active", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Username", StringComparison.OrdinalIgnoreCase) && 
                (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || 
                 errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Error: Ya existe otro usuario activo con ese nombre de usuario." });
            }
            
            if (errorMessage.Contains("UQ_User_Email_Active", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase) && 
                (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || 
                 errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Error: Ya existe otro usuario activo con ese correo electrónico." });
            }
            
            // Error genérico de duplicación
            if (errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Error: Ya existe otro usuario activo con estas credenciales." });
            }
            
            return StatusCode(500, new { 
                message = "Error al actualizar usuario en base de datos", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception genérica al actualizar usuario {UserId}. Tipo: {ExceptionType}", 
                id, ex.GetType().Name);
            
            return StatusCode(500, new { 
                message = "Error al actualizar usuario", 
                error = ex.Message,
                exceptionType = ex.GetType().Name,
                stackTrace = ex.StackTrace
            });
        }
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Obtener el usuario autenticado
            var currentUserIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "No se pudo identificar al usuario actual" });
            }

            // Validación 1: No permitir eliminar el propio usuario
            if (currentUserId == id)
            {
                return BadRequest(new { message = "No puedes eliminar tu propia cuenta mientras estás autenticado." });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Validación 2: Verificar si el usuario tiene sesión activa
            var hasActiveSession = await _context.Sessions
                .AnyAsync(s => s.UserId == id && s.IsActive == true);

            if (hasActiveSession)
            {
                return BadRequest(new { 
                    message = $"No se puede eliminar el usuario '{user.Username}' porque tiene una sesión activa. El usuario debe cerrar sesión primero." 
                });
            }

            // Validación 3: No permitir eliminar administradores
            if (user.Role != null && user.Role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { 
                    message = $"No se puede eliminar el usuario '{user.Username}' porque tiene rol de Administrador. Los administradores están protegidos." 
                });
            }

            // Eliminación lógica
            user.Status = false;
            user.UpdatedAt = DateTime.Now;
            user.CreatedUpdatedBy = currentUserId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario eliminado lógicamente: {UserId} por usuario {CurrentUserId}", id, currentUserId);
            return Ok(new { message = "Usuario eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario");
            return StatusCode(500, new { message = "Error al eliminar usuario", error = ex.Message });
        }
    }
}
