using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

using System.Security.Claims;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/users
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(UnilockerDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // =====================
    // CRUD ADMIN
    // =====================

    // GET: /api/users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _context.Users
            // .Where(u => u.Status)   // ← quita esta línea para ver activos e inactivos
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Status = u.Status,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null
            })
            .ToListAsync();

        return Ok(users);
    }


    // GET: /api/users/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _context.Users
            .Where(u => u.Status && u.Id == id)
            .Include(u => u.Role)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Status = u.Status,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: /api/users
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("El username es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("La contraseña es obligatoria.");

        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return BadRequest("El email ya está en uso.");

        var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists)
            return BadRequest("El username ya está en uso.");

        Role? role = null;
        if (dto.RoleId.HasValue)
        {
            role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId.Value && r.Status);
            if (role == null)
                return BadRequest("El rol especificado no existe o está inactivo.");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Status = true,
            RoleId = (int)dto.RoleId,
            FirstName = dto.Username,      // o "N/A" si prefieres
            LastName = dto.Username        // o string.Empty
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Status = user.Status,
            RoleId = user.RoleId,
            RoleName = role?.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FindAsync(id);
        if (user == null) // quitar || !user.Status
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("El username es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("El email es obligatorio.");

        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
        if (emailExists)
            return BadRequest("El email ya está en uso.");

        var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id);
        if (usernameExists)
            return BadRequest("El username ya está en uso.");

        Role? role = null;
        if (dto.RoleId.HasValue)
        {
            role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId.Value && r.Status);
            if (role == null)
                return BadRequest("El rol especificado no existe o está inactivo.");
        }

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.RoleId = (int)dto.RoleId;
        user.Status = dto.Status;              // ← aquí se actualiza el estado

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        }

        await _context.SaveChangesAsync();

        var result = await _context.Users
            .Where(u => u.Id == user.Id)
            .Include(u => u.Role)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Status = u.Status,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null
            })
            .FirstAsync();

        return Ok(result);
    }


    // DELETE: /api/users/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);   // ← borrar definitivo
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // =====================
    // PERFIL DEL USUARIO (UNI-67)
    // =====================

    // GET: /api/users/me
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!int.TryParse(userId, out var idValue))
            return Unauthorized();

        var user = await _context.Users
            .Where(u => u.Status && u.Id == idValue)
            .Include(u => u.Role)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Status = user.Status,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name
        };

        return Ok(dto);
    }


    public class ChangeMyPasswordRequest
    {
        public string? OldPassword { get; set; }  // opcional
        public string NewPassword { get; set; } = string.Empty;
    }

    // PUT: /api/users/me/password
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("Nueva contraseña requerida.");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!int.TryParse(userId, out var idValue))
            return Unauthorized();

        var user = await _context.Users.FindAsync(idValue);
        if (user == null || !user.Status)
            return NotFound();

        // Si quieres validar la contraseña actual, descomenta y ajusta:
        // if (!string.IsNullOrWhiteSpace(request.OldPassword))
        // {
        //     var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
        //     if (verification == PasswordVerificationResult.Failed)
        //         return BadRequest("La contraseña actual no es correcta.");
        // }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
