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
public class ClassroomsController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public ClassroomsController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/classrooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetAll()
    {
        var classrooms = await _context.Classrooms
            .Where(c => c.Status && c.Block.Status && c.Block.Branch.Status)
            .Include(c => c.Block)
            .ThenInclude(b => b.Branch)
            .OrderBy(c => c.Block.Branch.Name)
            .ThenBy(c => c.Block.Name)
            .ThenBy(c => c.Name)
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                BlockId = c.BlockId,
                BlockName = c.Block.Name,
                BranchId = c.Block.BranchId,
                BranchName = c.Block.Branch.Name
            })
            .ToListAsync();

        return Ok(classrooms);
    }

    // GET: /api/classrooms/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassroomDto>> GetById(int id)
    {
        var classroom = await _context.Classrooms
            .Where(c => c.Status && c.Id == id)
            .Include(c => c.Block)
            .ThenInclude(b => b.Branch)
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                BlockId = c.BlockId,
                BlockName = c.Block.Name,
                BranchId = c.Block.BranchId,
                BranchName = c.Block.Branch.Name
            })
            .FirstOrDefaultAsync();

        if (classroom == null)
            return NotFound();

        return Ok(classroom);
    }

    // POST: /api/classrooms
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassroomDto>> Create([FromBody] ClassroomCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var block = await _context.Blocks
            .Include(b => b.Branch)
            .FirstOrDefaultAsync(b => b.Id == dto.BlockId && b.Status && b.Branch.Status);

        if (block == null)
            return BadRequest("El bloque especificado no existe o está inactivo.");

        var entity = new Classroom
        {
            Name = dto.Name,
            BlockId = dto.BlockId,
            Status = true
        };

        _context.Classrooms.Add(entity);

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        entity = await _context.Classrooms
            .Include(c => c.Block)
            .ThenInclude(b => b.Branch)
            .FirstAsync(c => c.Id == entity.Id);

        var result = new ClassroomDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            BlockId = entity.BlockId,
            BlockName = entity.Block.Name,
            BranchId = entity.Block.BranchId,
            BranchName = entity.Block.Branch.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/classrooms/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassroomDto>> Update(int id, [FromBody] ClassroomCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        var entity = await _context.Classrooms.FindAsync(id);
        if (entity == null || !entity.Status)
            return NotFound();

        var block = await _context.Blocks
            .Include(b => b.Branch)
            .FirstOrDefaultAsync(b => b.Id == dto.BlockId && b.Status && b.Branch.Status);

        if (block == null)
            return BadRequest("El bloque especificado no existe o está inactivo.");

        entity.Name = dto.Name;
        entity.BlockId = dto.BlockId;

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        entity = await _context.Classrooms
            .Include(c => c.Block)
            .ThenInclude(b => b.Branch)
            .FirstAsync(c => c.Id == entity.Id);

        var result = new ClassroomDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Status = entity.Status,
            BlockId = entity.BlockId,
            BlockName = entity.Block.Name,
            BranchId = entity.Block.BranchId,
            BranchName = entity.Block.Branch.Name
        };

        return Ok(result);
    }

    // DELETE: /api/classrooms/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Classrooms.FindAsync(id);
        if (entity == null || !entity.Status)
            return NotFound();

        entity.Status = false;

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
