using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClassroomsController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<ClassroomsController> _logger;

    public ClassroomsController(UnilockerDbContext context, ILogger<ClassroomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/classrooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetClassrooms()
    {
        try
        {
            var classrooms = await _context.Classrooms
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Capacity,
                    c.BlockId,
                    BlockName = _context.Blocks.Where(b => b.Id == c.BlockId).Select(b => b.Name).FirstOrDefault(),
                    BranchId = _context.Blocks.Where(b => b.Id == c.BlockId).Select(b => b.BranchId).FirstOrDefault(),
                    BranchName = _context.Blocks.Where(b => b.Id == c.BlockId)
                        .Select(b => _context.Branches.Where(br => br.Id == b.BranchId).Select(br => br.Name).FirstOrDefault())
                        .FirstOrDefault(),
                    c.Status,
                    c.CreatedAt,
                    c.UpdatedAt,
                    ComputerCount = _context.Computers.Count(comp => comp.ClassroomId == c.Id)
                })
                .ToListAsync();

            return Ok(classrooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener aulas");
            return StatusCode(500, new { message = "Error al obtener aulas", error = ex.Message });
        }
    }

    // GET: api/classrooms/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetClassroom(int id)
    {
        try
        {
            var classroom = await _context.Classrooms
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Capacity,
                    c.BlockId,
                    BlockName = _context.Blocks.Where(b => b.Id == c.BlockId).Select(b => b.Name).FirstOrDefault(),
                    BranchId = _context.Blocks.Where(b => b.Id == c.BlockId).Select(b => b.BranchId).FirstOrDefault(),
                    BranchName = _context.Blocks.Where(b => b.Id == c.BlockId)
                        .Select(b => _context.Branches.Where(br => br.Id == b.BranchId).Select(br => br.Name).FirstOrDefault())
                        .FirstOrDefault(),
                    c.Status,
                    c.CreatedAt,
                    c.UpdatedAt,
                    Computers = _context.Computers.Where(comp => comp.ClassroomId == c.Id).Select(comp => new { comp.Id, comp.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            if (classroom == null)
            {
                return NotFound(new { message = "Aula no encontrada" });
            }

            return Ok(classroom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener aula");
            return StatusCode(500, new { message = "Error al obtener aula", error = ex.Message });
        }
    }

    // POST: api/classrooms
    [HttpPost]
    public async Task<ActionResult<Classroom>> CreateClassroom([FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("CreateClassroom - Payload: {Payload}", dto.ToString());

            var name = dto.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : string.Empty;
            var capacity = dto.TryGetProperty("capacity", out var capacityEl) && capacityEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? capacityEl.GetInt32() : (int?)null;
            var blockId = dto.TryGetProperty("blockId", out var blockEl) ? blockEl.GetInt32() : 0;
            var status = dto.TryGetProperty("status", out var statusEl) ? statusEl.GetBoolean() : true;

            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }
            if (capacity.HasValue && capacity.Value > 100)
            {
                return BadRequest(new { message = "La capacidad no puede ser mayor a 100" });
            }
            if (blockId == 0)
            {
                return BadRequest(new { message = "BlockId es obligatorio" });
            }

            var classroom = new Classroom
            {
                Name = name,
                Capacity = capacity,
                BlockId = blockId,
                Status = status,
                CreatedAt = DateTime.Now
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Aula creada exitosamente: {ClassroomId}", classroom.Id);
            return CreatedAtAction(nameof(GetClassroom), new { id = classroom.Id }, classroom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear aula");
            return StatusCode(500, new { message = "Error al crear aula", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // PUT: api/classrooms/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClassroom(int id, [FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("UpdateClassroom - ID: {Id}, Payload: {Payload}", id, dto.ToString());

            var existingClassroom = await _context.Classrooms.FindAsync(id);
            if (existingClassroom == null)
            {
                return NotFound(new { message = "Aula no encontrada" });
            }

            if (dto.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingClassroom.Name = nameEl.GetString() ?? existingClassroom.Name;
            }
            if (dto.TryGetProperty("capacity", out var capacityEl))
            {
                var newCapacity = capacityEl.ValueKind != System.Text.Json.JsonValueKind.Null ? capacityEl.GetInt32() : (int?)null;
                if (newCapacity.HasValue && newCapacity.Value > 100)
                {
                    return BadRequest(new { message = "La capacidad no puede ser mayor a 100" });
                }
                existingClassroom.Capacity = newCapacity;
            }
            if (dto.TryGetProperty("blockId", out var blockEl))
            {
                existingClassroom.BlockId = blockEl.GetInt32();
            }
            if (dto.TryGetProperty("status", out var statusEl))
            {
                existingClassroom.Status = statusEl.GetBoolean();
            }
            existingClassroom.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Aula actualizada exitosamente: {ClassroomId}", id);
            return Ok(existingClassroom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar aula");
            return StatusCode(500, new { message = "Error al actualizar aula", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // DELETE: api/classrooms/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClassroom(int id)
    {
        try
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound(new { message = "Aula no encontrada" });
            }

            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aula eliminada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar aula");
            return StatusCode(500, new { message = "Error al eliminar aula", error = ex.Message });
        }
    }
}
