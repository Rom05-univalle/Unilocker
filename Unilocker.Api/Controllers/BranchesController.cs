using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Helpers;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(UnilockerDbContext context, ILogger<BranchesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/branches
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetBranches()
    {
        try
        {
            var branches = await _context.Branches
                .Where(b => b.Status == true)
                .OrderBy(b => b.Name)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    b.Code,
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    BlockCount = b.Blocks.Count(bl => bl.Status == true)
                })
                .ToListAsync();

            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sedes");
            return StatusCode(500, new { message = "Error al obtener sedes", error = ex.Message });
        }
    }

    // GET: api/branches/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBranch(int id)
    {
        try
        {
            var branch = await _context.Branches
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    b.Code,
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    Blocks = b.Blocks.Select(bl => new { bl.Id, bl.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            if (branch == null)
            {
                return NotFound(new { message = "Sede no encontrada" });
            }

            return Ok(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sede");
            return StatusCode(500, new { message = "Error al obtener sede", error = ex.Message });
        }
    }

    // POST: api/branches
    [HttpPost]
    public async Task<ActionResult<Branch>> CreateBranch(Branch branch)
    {
        try
        {
            // Normalizar campos de texto
            branch.Name = StringNormalizer.Normalize(branch.Name);
            branch.Address = StringNormalizer.Normalize(branch.Address);
            branch.Code = StringNormalizer.Normalize(branch.Code);

            branch.CreatedAt = DateTime.Now;
            branch.Status = true;
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sede");
            return StatusCode(500, new { message = "Error al crear sede", error = ex.Message });
        }
    }

    // PUT: api/branches/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(int id, Branch branch)
    {
        if (id != branch.Id)
        {
            return BadRequest(new { message = "ID no coincide" });
        }

        try
        {
            var existingBranch = await _context.Branches.FindAsync(id);
            if (existingBranch == null)
            {
                return NotFound(new { message = "Sede no encontrada" });
            }

            existingBranch.Name = StringNormalizer.Normalize(branch.Name);
            existingBranch.Address = StringNormalizer.Normalize(branch.Address);
            existingBranch.Code = StringNormalizer.Normalize(branch.Code);
            existingBranch.Status = branch.Status;
            existingBranch.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(existingBranch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sede");
            return StatusCode(500, new { message = "Error al actualizar sede", error = ex.Message });
        }
    }

    // DELETE: api/branches/5 (Eliminación lógica en cascada)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(new { message = "Sede no encontrada" });
            }

            // Obtener todos los bloques de esta sede (activos e inactivos)
            var blocks = await _context.Blocks
                .Where(b => b.BranchId == id)
                .ToListAsync();

            // Para cada bloque, verificar si tiene aulas con computadoras activas
            foreach (var block in blocks)
            {
                var computersCount = await _context.Computers
                    .Join(_context.Classrooms,
                        comp => comp.ClassroomId,
                        classroom => classroom.Id,
                        (comp, classroom) => new { comp, classroom })
                    .Where(x => x.classroom.BlockId == block.Id && x.comp.Status == true)
                    .CountAsync();

                if (computersCount > 0)
                {
                    return BadRequest(new { 
                        message = $"No se puede eliminar la sede '{branch.Name}' porque el bloque '{block.Name}' tiene {computersCount} computadora(s) activa(s) registrada(s). Desregistre las computadoras primero." 
                    });
                }
            }

            // Si no hay computadoras activas, proceder con eliminación en cascada
            // 1. Eliminar lógicamente todas las aulas de todos los bloques
            var classrooms = await _context.Classrooms
                .Where(c => blocks.Select(b => b.Id).Contains(c.BlockId))
                .ToListAsync();

            foreach (var classroom in classrooms)
            {
                classroom.Status = false;
                classroom.UpdatedAt = DateTime.Now;
            }

            // 2. Eliminar lógicamente todos los bloques
            foreach (var block in blocks)
            {
                block.Status = false;
                block.UpdatedAt = DateTime.Now;
            }

            // 3. Eliminar lógicamente la sede
            branch.Status = false;
            branch.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sede eliminada lógicamente en cascada: {Id}, Bloques: {BlockCount}, Aulas: {ClassroomCount}", 
                id, blocks.Count, classrooms.Count);
            return Ok(new { 
                message = "Sede eliminada correctamente", 
                blocksDeleted = blocks.Count, 
                classroomsDeleted = classrooms.Count 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar sede");
            return StatusCode(500, new { message = "Error al eliminar sede", error = ex.Message });
        }
    }
}
