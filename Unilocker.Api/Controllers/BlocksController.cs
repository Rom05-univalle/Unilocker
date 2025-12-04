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
public class BlocksController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public BlocksController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/blocks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlockDto>>> GetAll([FromQuery] int? branchId)
    {
        var query = _context.Blocks
            .Include(b => b.Branch)
            .Where(b => b.Branch.Status) // solo sucursales activas
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId.Value);
        }

        var blocks = await query
            .OrderBy(b => b.Branch.Name)
            .ThenBy(b => b.Name)
            .Select(b => new BlockDto
            {
                Id = b.Id,
                Name = b.Name,
                Status = b.Status, // true = Activo, false = Inactivo
                BranchId = b.BranchId,
                BranchName = b.Branch.Name
            })
            .ToListAsync();

        return Ok(blocks);
    }

    // GET: /api/blocks/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlockDto>> GetById(int id)
    {
        var block = await _context.Blocks
            .Where(b => b.Status && b.Id == id)
            .Include(b => b.Branch)
            .Select(b => new BlockDto
            {
                Id = b.Id,
                Name = b.Name,
                Status = b.Status,
                BranchId = b.BranchId,
                BranchName = b.Branch.Name
            })
            .FirstOrDefaultAsync();

        if (block == null)
            return NotFound();

        return Ok(block);
    }

    // POST: /api/blocks
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BlockDto>> Create([FromBody] BlockCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId && b.Status);
        if (!branchExists)
            return BadRequest("La sucursal especificada no existe o está inactiva.");

        var entity = new Block
        {
            Name = dto.Name,
            BranchId = dto.BranchId,
            Status = true
        };

        _context.Blocks.Add(entity);

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        // recargar con Include para obtener BranchName
        entity = await _context.Blocks
            .Include(b => b.Branch)
            .FirstAsync(b => b.Id == entity.Id);

        var result = new BlockDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            BranchId = entity.BranchId,
            BranchName = entity.Branch.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/blocks/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BlockDto>> Update(int id, [FromBody] BlockCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var entity = await _context.Blocks.FindAsync(id);
        if (entity == null)
            return NotFound();

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId && b.Status);
        if (!branchExists)
            return BadRequest("La sucursal especificada no existe o está inactiva.");

        entity.Name = dto.Name;
        entity.BranchId = dto.BranchId;
        entity.Status = dto.status; // asegúrate que la propiedad en el DTO sea 'Status'

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        entity = await _context.Blocks
            .Include(b => b.Branch)
            .FirstAsync(b => b.Id == entity.Id);

        var result = new BlockDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            BranchId = entity.BranchId,
            BranchName = entity.Branch.Name
        };

        return Ok(result);
    }

    // DELETE: /api/blocks/5 (borrado definitivo)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Blocks.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Blocks.Remove(entity); // elimina físicamente el bloque

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
