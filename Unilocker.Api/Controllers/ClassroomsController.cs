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
                    BlockName = c.Block != null ? c.Block.Name : null,
                    BranchName = c.Block != null && c.Block.Branch != null ? c.Block.Branch.Name : null,
                    c.Status,
                    c.CreatedAt,
                    c.UpdatedAt,
                    ComputerCount = c.Computers.Count
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
                    BlockName = c.Block != null ? c.Block.Name : null,
                    BranchName = c.Block != null && c.Block.Branch != null ? c.Block.Branch.Name : null,
                    c.Status,
                    c.CreatedAt,
                    c.UpdatedAt,
                    Computers = c.Computers.Select(comp => new { comp.Id, comp.Name }).ToList()
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
    public async Task<ActionResult<Classroom>> CreateClassroom(Classroom classroom)
    {
        try
        {
            classroom.CreatedAt = DateTime.Now;
            classroom.Status = true;
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClassroom), new { id = classroom.Id }, classroom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear aula");
            return StatusCode(500, new { message = "Error al crear aula", error = ex.Message });
        }
    }

    // PUT: api/classrooms/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClassroom(int id, Classroom classroom)
    {
        if (id != classroom.Id)
        {
            return BadRequest(new { message = "ID no coincide" });
        }

        try
        {
            var existingClassroom = await _context.Classrooms.FindAsync(id);
            if (existingClassroom == null)
            {
                return NotFound(new { message = "Aula no encontrada" });
            }

            existingClassroom.Name = classroom.Name;
            existingClassroom.Capacity = classroom.Capacity;
            existingClassroom.BlockId = classroom.BlockId;
            existingClassroom.Status = classroom.Status;
            existingClassroom.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(existingClassroom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar aula");
            return StatusCode(500, new { message = "Error al actualizar aula", error = ex.Message });
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
