using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public BranchesController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/branches
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetAll()
    {
        var branches = await _context.Branches
            // ya NO filtramos por Status para poder ver inactivas
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Code = b.Code,
                Status = b.Status
            })
            .ToListAsync();

        return Ok(branches);
    }

    // GET: /api/branches/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BranchDto>> GetById(int id)
    {
        var branch = await _context.Branches
            .Where(b => b.Id == id)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Code = b.Code,
                Status = b.Status
            })
            .FirstOrDefaultAsync();

        if (branch == null)
            return NotFound();

        return Ok(branch);
    }

    // POST: /api/branches
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BranchDto>> Create([FromBody] BranchCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var entity = new Branch
        {
            Name = dto.Name,
            Address = dto.Address,
            Code = dto.Code,
            Status = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _context.Branches.Add(entity);

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        var result = new BranchDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Code = entity.Code,
            Status = entity.Status
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/branches/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BranchDto>> Update(int id, [FromBody] BranchCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.Code = dto.Code;
        entity.Status = dto.Status; // aquí se actualiza el estado
        entity.UpdatedAt = DateTime.UtcNow;

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        var result = new BranchDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Code = entity.Code,
            Status = entity.Status
        };

        return Ok(result);
    }

    // DELETE: /api/branches/5 (delete definitivo)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Branches.Remove(entity); // elimina el registro de la tabla

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
