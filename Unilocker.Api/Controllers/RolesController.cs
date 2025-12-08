using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(UnilockerDbContext context, ILogger<RolesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Where(r => r.Status == true)
                .OrderBy(r => r.Name)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Status,
                    r.CreatedAt,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles");
            return StatusCode(500, new { message = "Error al obtener roles", error = ex.Message });
        }
    }

    // GET: api/roles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetRole(int id)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Status,
                    r.CreatedAt,
                    r.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol");
            return StatusCode(500, new { message = "Error al obtener rol", error = ex.Message });
        }
    }

    // POST: api/roles
    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole(Role role)
    {
        try
        {
            role.CreatedAt = DateTime.Now;
            role.Status = true;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol");
            return StatusCode(500, new { message = "Error al crear rol", error = ex.Message });
        }
    }

    // PUT: api/roles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, Role role)
    {
        if (id != role.Id)
        {
            return BadRequest(new { message = "ID no coincide" });
        }

        try
        {
            var existingRole = await _context.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            // Validación: No permitir cambiar el nombre del rol Administrador
            if (existingRole.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase) && 
                !role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "No puedes cambiar el nombre del rol Administrador" });
            }

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            existingRole.Status = role.Status;
            existingRole.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rol actualizado: {RoleId}", id);
            return Ok(existingRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol");
            return StatusCode(500, new { message = "Error al actualizar rol", error = ex.Message });
        }
    }

    // DELETE: api/roles/5 (Eliminación en cascada)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            // Validación 1: No permitir eliminar el rol Administrador
            if (role.Name.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"No se puede eliminar el rol '{role.Name}' porque es un rol del sistema protegido." });
            }

            // Validación 2: Verificar si algún usuario con este rol tiene sesión activa
            var usersWithRole = await _context.Users
                .Where(u => u.RoleId == id)
                .ToListAsync();

            if (usersWithRole.Any())
            {
                // Verificar si hay sesiones activas de estos usuarios
                var userIds = usersWithRole.Select(u => u.Id).ToList();
                var activeSessionsCount = await _context.Sessions
                    .CountAsync(s => userIds.Contains(s.UserId) && s.IsActive == true);

                if (activeSessionsCount > 0)
                {
                    return BadRequest(new { 
                        message = $"No se puede eliminar el rol '{role.Name}' porque hay {activeSessionsCount} usuario(s) con sesiones activas. Los usuarios deben cerrar sesión primero." 
                    });
                }

                // Si no hay sesiones activas, eliminar lógicamente todos los usuarios del rol
                foreach (var user in usersWithRole)
                {
                    user.Status = false;
                    user.UpdatedAt = DateTime.Now;
                }
            }

            // Eliminación lógica del rol
            role.Status = false;
            role.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rol eliminado lógicamente en cascada: {RoleId}, Usuarios eliminados: {UserCount}", 
                id, usersWithRole.Count);
            return Ok(new { 
                message = "Rol eliminado correctamente", 
                usersDeleted = usersWithRole.Count 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol");
            return StatusCode(500, new { message = "Error al eliminar rol", error = ex.Message });
        }
    }
}
