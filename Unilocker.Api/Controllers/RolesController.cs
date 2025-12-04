using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/roles
[Authorize]
public class RolesController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public RolesController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/roles
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var items = await _context.Roles
            // .Where(r => r.Status)  // mostramos activos e inactivos
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                Description = r.Description
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/roles/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var item = await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                Description = r.Description
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST: /api/roles
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoleDto>> Create([FromBody] RoleCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        // solo bloquear duplicados activos
        var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name && r.Status);
        if (exists)
            return BadRequest("Ya existe un rol activo con ese nombre.");

        var entity = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status   // o true si quieres forzar que siempre se cree activo
        };

        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();

        var result = new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            Description = entity.Description
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/roles/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoleDto>> Update(int id, [FromBody] RoleCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _context.Roles.FindAsync(id);
        if (entity == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        // validación de nombre contra otros roles activos
        var exists = await _context.Roles
            .AnyAsync(r => r.Name == dto.Name && r.Id != id && r.Status);
        if (exists)
            return BadRequest("Ya existe otro rol activo con ese nombre.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Status = dto.Status;

        await _context.SaveChangesAsync();

        var result = new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            Description = entity.Description
        };

        return Ok(result);
    }

    // DELETE: /api/roles/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Roles.FindAsync(id);
        if (entity == null)
            return NotFound();

        var hasUsers = await _context.Users.AnyAsync(u => u.RoleId == id && u.Status);
        if (hasUsers)
            return BadRequest("No se puede eliminar el rol porque tiene usuarios asociados.");

        _context.Roles.Remove(entity);   // ← borrado físico
        await _context.SaveChangesAsync();

        return NoContent();
    }

}
