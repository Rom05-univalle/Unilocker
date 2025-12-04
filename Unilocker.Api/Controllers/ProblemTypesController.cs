using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;
using DtoProblemType = Unilocker.Api.DTOs.ProblemTypeDto;
using DtoProblemTypeCreateUpdate = Unilocker.Api.DTOs.ProblemTypeCreateUpdateDto;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/problemtypes
[Authorize]
public class ProblemTypesController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public ProblemTypesController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/problemtypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DtoProblemType>>> GetAll()
    {
        var items = await _context.ProblemTypes
            // .Where(p => p.Status) // descomenta si quieres solo activos
            .OrderBy(p => p.Name)
            .Select(p => new DtoProblemType
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/problemtypes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DtoProblemType>> GetById(int id)
    {
        var item = await _context.ProblemTypes
            // .Where(p => p.Status && p.Id == id)
            .Where(p => p.Id == id)
            .Select(p => new DtoProblemType
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.Status
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST: /api/problemtypes
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DtoProblemType>> Create([FromBody] DtoProblemTypeCreateUpdate dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var exists = await _context.ProblemTypes.AnyAsync(p => p.Name == dto.Name);
        if (exists)
            return BadRequest("Ya existe un tipo de problema con ese nombre.");

        var entity = new ProblemType
        {
            Name = dto.Name,
            Status = dto.Status
        };

        _context.ProblemTypes.Add(entity);

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        var result = new DtoProblemType
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/problemtypes/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DtoProblemType>> Update(int id, [FromBody] DtoProblemTypeCreateUpdate dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _context.ProblemTypes.FindAsync(id);
        if (entity == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var exists = await _context.ProblemTypes.AnyAsync(p => p.Name == dto.Name && p.Id != id);
        if (exists)
            return BadRequest("Ya existe otro tipo de problema con ese nombre.");

        entity.Name = dto.Name;
        entity.Status = dto.Status;

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        var result = new DtoProblemType
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status
        };

        return Ok(result);
    }

    // DELETE: /api/problemtypes/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.ProblemTypes.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.ProblemTypes.Remove(entity); // borrado físico

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
