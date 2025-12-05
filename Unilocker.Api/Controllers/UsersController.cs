using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UnilockerDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.FirstName,
                    u.LastName,
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
                    u.FirstName,
                    u.LastName,
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

            var username = dto.TryGetProperty("username", out var usernameEl) ? usernameEl.GetString() : string.Empty;
            var email = dto.TryGetProperty("email", out var emailEl) && emailEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? emailEl.GetString() : null;
            var firstName = dto.TryGetProperty("firstName", out var firstNameEl) ? firstNameEl.GetString() : string.Empty;
            var lastName = dto.TryGetProperty("lastName", out var lastNameEl) ? lastNameEl.GetString() : string.Empty;
            var passwordHash = dto.TryGetProperty("passwordHash", out var passwordEl) ? passwordEl.GetString() : null;
            var roleId = dto.TryGetProperty("roleId", out var roleEl) ? roleEl.GetInt32() : 0;
            var status = dto.TryGetProperty("status", out var statusEl) ? statusEl.GetBoolean() : true;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "El username es obligatorio" });
            }
            if (string.IsNullOrEmpty(firstName))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }
            if (string.IsNullOrEmpty(lastName))
            {
                return BadRequest(new { message = "El apellido es obligatorio" });
            }
            if (string.IsNullOrEmpty(passwordHash))
            {
                return BadRequest(new { message = "La contraseña es obligatoria" });
            }
            if (roleId == 0)
            {
                return BadRequest(new { message = "RoleId es obligatorio" });
            }

            var user = new User
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordHash),
                RoleId = roleId,
                Status = status,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario creado exitosamente: {UserId}", user.Id);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.RoleId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("UpdateUser - ID: {Id}, Payload: {Payload}", id, dto.ToString());

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            if (dto.TryGetProperty("username", out var usernameEl) && usernameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingUser.Username = usernameEl.GetString() ?? existingUser.Username;
            }
            if (dto.TryGetProperty("email", out var emailEl))
            {
                existingUser.Email = emailEl.ValueKind != System.Text.Json.JsonValueKind.Null ? emailEl.GetString() : null;
            }
            if (dto.TryGetProperty("firstName", out var firstNameEl) && firstNameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingUser.FirstName = firstNameEl.GetString() ?? existingUser.FirstName;
            }
            if (dto.TryGetProperty("lastName", out var lastNameEl) && lastNameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingUser.LastName = lastNameEl.GetString() ?? existingUser.LastName;
            }
            if (dto.TryGetProperty("roleId", out var roleEl))
            {
                existingUser.RoleId = roleEl.GetInt32();
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
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordEl.GetString());
            }
            
            existingUser.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", id);
            return Ok(new
            {
                existingUser.Id,
                existingUser.Username,
                existingUser.Email,
                existingUser.FirstName,
                existingUser.LastName,
                existingUser.RoleId,
                existingUser.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario");
            return StatusCode(500, new { message = "Error al actualizar usuario", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario");
            return StatusCode(500, new { message = "Error al eliminar usuario", error = ex.Message });
        }
    }
}
