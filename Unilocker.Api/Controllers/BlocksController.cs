using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BlocksController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<BlocksController> _logger;

    public BlocksController(UnilockerDbContext context, ILogger<BlocksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/blocks
    // GET: api/blocks?branchId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetBlocks([FromQuery] int? branchId)
    {
        try
        {
            var query = _context.Blocks.Where(b => b.Status == true);

            if (branchId.HasValue)
            {
                query = query.Where(b => b.BranchId == branchId.Value);
            }

            var blocks = await query
                .OrderBy(b => b.Name)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    b.BranchId,
                    BranchName = _context.Branches.Where(br => br.Id == b.BranchId).Select(br => br.Name).FirstOrDefault(),
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    ClassroomCount = _context.Classrooms.Count(c => c.BlockId == b.Id && c.Status == true)
                })
                .ToListAsync();

            return Ok(blocks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bloques");
            return StatusCode(500, new { message = "Error al obtener bloques", error = ex.Message });
        }
    }

    // GET: api/blocks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBlock(int id)
    {
        try
        {
            var block = await _context.Blocks
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    b.BranchId,
                    BranchName = _context.Branches.Where(br => br.Id == b.BranchId).Select(br => br.Name).FirstOrDefault(),
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    Classrooms = _context.Classrooms.Where(c => c.BlockId == b.Id).Select(c => new { c.Id, c.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            if (block == null)
            {
                return NotFound(new { message = "Bloque no encontrado" });
            }

            return Ok(block);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bloque");
            return StatusCode(500, new { message = "Error al obtener bloque", error = ex.Message });
        }
    }

    // POST: api/blocks
    [HttpPost]
    public async Task<ActionResult<Block>> CreateBlock([FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("CreateBlock - Payload recibido: {Payload}", dto.ToString());

            var name = dto.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : string.Empty;
            var address = dto.TryGetProperty("address", out var addressEl) && addressEl.ValueKind != System.Text.Json.JsonValueKind.Null 
                ? addressEl.GetString() : null;
            var branchId = dto.TryGetProperty("branchId", out var branchEl) ? branchEl.GetInt32() : 0;
            var status = dto.TryGetProperty("status", out var statusEl) ? statusEl.GetBoolean() : true;

            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }
            if (branchId == 0)
            {
                return BadRequest(new { message = "BranchId es obligatorio" });
            }

            var block = new Block
            {
                Name = name,
                Address = address,
                BranchId = branchId,
                Status = status,
                CreatedAt = DateTime.Now
            };

            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bloque creado exitosamente: {BlockId}", block.Id);
            return CreatedAtAction(nameof(GetBlock), new { id = block.Id }, block);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear bloque");
            return StatusCode(500, new { message = "Error al crear bloque", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // PUT: api/blocks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlock(int id, [FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("UpdateBlock - ID: {Id}, Payload: {Payload}", id, dto.ToString());

            var existingBlock = await _context.Blocks.FindAsync(id);
            if (existingBlock == null)
            {
                return NotFound(new { message = "Bloque no encontrado" });
            }

            if (dto.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingBlock.Name = nameEl.GetString() ?? existingBlock.Name;
            }
            if (dto.TryGetProperty("address", out var addressEl))
            {
                existingBlock.Address = addressEl.ValueKind != System.Text.Json.JsonValueKind.Null ? addressEl.GetString() : null;
            }
            if (dto.TryGetProperty("branchId", out var branchEl))
            {
                existingBlock.BranchId = branchEl.GetInt32();
            }
            if (dto.TryGetProperty("status", out var statusEl))
            {
                existingBlock.Status = statusEl.GetBoolean();
            }
            existingBlock.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bloque actualizado exitosamente: {BlockId}", id);
            return Ok(existingBlock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar bloque");
            return StatusCode(500, new { message = "Error al actualizar bloque", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // DELETE: api/blocks/5 (Eliminación lógica)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlock(int id)
    {
        try
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null)
            {
                return NotFound(new { message = "Bloque no encontrado" });
            }

            // Verificar si hay aulas activas en este bloque
            var activeClassrooms = await _context.Classrooms
                .Where(c => c.BlockId == id && c.Status == true)
                .ToListAsync();

            if (activeClassrooms.Any())
            {
                // Verificar si alguna aula tiene computadoras activas
                var classroomIds = activeClassrooms.Select(c => c.Id).ToList();
                var hasActiveComputers = await _context.Computers
                    .AnyAsync(comp => classroomIds.Contains(comp.ClassroomId) && comp.Status == true);

                if (hasActiveComputers)
                {
                    return BadRequest(new { 
                        message = $"No se puede eliminar el bloque '{block.Name}' porque tiene aulas con computadoras activas registradas. Desregistre las computadoras primero." 
                    });
                }

                return BadRequest(new { 
                    message = $"No se puede eliminar el bloque '{block.Name}' porque tiene {activeClassrooms.Count} aula(s) activa(s). Elimine las aulas primero." 
                });
            }

            // Eliminación lógica
            block.Status = false;
            block.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bloque eliminado lógicamente: {Id}", id);
            return Ok(new { message = "Bloque eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar bloque");
            return StatusCode(500, new { message = "Error al eliminar bloque", error = ex.Message });
        }
    }
}
