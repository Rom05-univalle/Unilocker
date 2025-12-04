using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/computers
[Authorize]
public class ComputersController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public ComputersController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/computers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComputerDto>>> GetAll()
    {
        var items = await _context.Computers
            .Include(c => c.Classroom)
            .ThenInclude(cl => cl.Block)
            .ThenInclude(b => b.Branch)
            .OrderBy(c => c.Name)
            .Select(c => new ComputerDto
            {
                Id = c.Id,
                Name = c.Name,
                Uuid = c.Uuid,
                Status = c.Status,
                // Campos de ubicación
                ClassroomId = c.ClassroomId,
                ClassroomName = c.Classroom.Name,
                BlockId = c.Classroom.BlockId,
                BlockName = c.Classroom.Block.Name,
                BranchId = c.Classroom.Block.BranchId,
                BranchName = c.Classroom.Block.Branch.Name
                // Brand/Model/SerialNumber NO se usan porque no existen en la entidad
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/computers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComputerDto>> GetById(int id)
    {
        var item = await _context.Computers
            .Where(c => c.Id == id)
            .Include(c => c.Classroom)
            .ThenInclude(cl => cl.Block)
            .ThenInclude(b => b.Branch)
            .Select(c => new ComputerDto
            {
                Id = c.Id,
                Name = c.Name,
                Uuid = c.Uuid,
                Status = c.Status,
                ClassroomId = c.ClassroomId,
                ClassroomName = c.Classroom.Name,
                BlockId = c.Classroom.BlockId,
                BlockName = c.Classroom.Block.Name,
                BranchId = c.Classroom.Block.BranchId,
                BranchName = c.Classroom.Block.Branch.Name
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST: /api/computers
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComputerDto>> Create([FromBody] ComputerCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        if (dto.Uuid == Guid.Empty)
            return BadRequest("El UUID es obligatorio.");

        var uuidExists = await _context.Computers.AnyAsync(c => c.Uuid == dto.Uuid);
        if (uuidExists)
            return BadRequest("Ya existe una computadora con ese UUID.");

        // validar aula
        var classroomExists = await _context.Classrooms
            .AnyAsync(c => c.Id == dto.ClassroomId && c.Status);
        if (!classroomExists)
            return BadRequest("El aula especificada no existe o está inactiva.");

        var entity = new Computer
        {
            Name = dto.Name,
            Uuid = dto.Uuid,
            Status = dto.Status,
            ClassroomId = dto.ClassroomId
            // Brand/Model/SerialNumber no existen en Computer, por eso no se asignan
        };

        _context.Computers.Add(entity);

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        entity = await _context.Computers
            .Include(c => c.Classroom)
            .ThenInclude(cl => cl.Block)
            .ThenInclude(b => b.Branch)
            .FirstAsync(c => c.Id == entity.Id);

        var result = new ComputerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Uuid = entity.Uuid,
            Status = entity.Status,
            ClassroomId = entity.ClassroomId,
            ClassroomName = entity.Classroom.Name,
            BlockId = entity.Classroom.BlockId,
            BlockName = entity.Classroom.Block.Name,
            BranchId = entity.Classroom.Block.BranchId,
            BranchName = entity.Classroom.Block.Branch.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: /api/computers/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComputerDto>> Update(int id, [FromBody] ComputerCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _context.Computers.FindAsync(id);
        if (entity == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        if (dto.Uuid == Guid.Empty)
            return BadRequest("El UUID es obligatorio.");

        var uuidExists = await _context.Computers
            .AnyAsync(c => c.Uuid == dto.Uuid && c.Id != id);
        if (uuidExists)
            return BadRequest("Ya existe otra computadora con ese UUID.");

        var classroomExists = await _context.Classrooms
            .AnyAsync(c => c.Id == dto.ClassroomId && c.Status);
        if (!classroomExists)
            return BadRequest("El aula especificada no existe o está inactiva.");

        entity.Name = dto.Name;
        entity.Uuid = dto.Uuid;
        entity.Status = dto.Status;
        entity.ClassroomId = dto.ClassroomId;

        // ==== Auditoría ====
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
        {
            _context.CurrentUserId = uid;
        }
        _context.CurrentIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ====================

        await _context.SaveChangesAsync();

        entity = await _context.Computers
            .Include(c => c.Classroom)
            .ThenInclude(cl => cl.Block)
            .ThenInclude(b => b.Branch)
            .FirstAsync(c => c.Id == entity.Id);

        var result = new ComputerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Uuid = entity.Uuid,
            Status = entity.Status,
            ClassroomId = entity.ClassroomId,
            ClassroomName = entity.Classroom.Name,
            BlockId = entity.Classroom.BlockId,
            BlockName = entity.Classroom.Block.Name,
            BranchId = entity.Classroom.Block.BranchId,
            BranchName = entity.Classroom.Block.Branch.Name
        };

        return Ok(result);
    }

    // DELETE: /api/computers/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Computers.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Computers.Remove(entity); // borrado físico

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
