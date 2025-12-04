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
            var query = _context.Blocks.AsQueryable();

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
                    BranchName = b.Branch != null ? b.Branch.Name : null,
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    ClassroomCount = b.Classrooms.Count
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
                    BranchName = b.Branch != null ? b.Branch.Name : null,
                    b.Status,
                    b.CreatedAt,
                    b.UpdatedAt,
                    Classrooms = b.Classrooms.Select(c => new { c.Id, c.Name }).ToList()
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
    public async Task<ActionResult<Block>> CreateBlock(Block block)
    {
        try
        {
            block.CreatedAt = DateTime.Now;
            block.Status = true;
            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlock), new { id = block.Id }, block);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear bloque");
            return StatusCode(500, new { message = "Error al crear bloque", error = ex.Message });
        }
    }

    // PUT: api/blocks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlock(int id, Block block)
    {
        if (id != block.Id)
        {
            return BadRequest(new { message = "ID no coincide" });
        }

        try
        {
            var existingBlock = await _context.Blocks.FindAsync(id);
            if (existingBlock == null)
            {
                return NotFound(new { message = "Bloque no encontrado" });
            }

            existingBlock.Name = block.Name;
            existingBlock.Address = block.Address;
            existingBlock.BranchId = block.BranchId;
            existingBlock.Status = block.Status;
            existingBlock.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(existingBlock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar bloque");
            return StatusCode(500, new { message = "Error al actualizar bloque", error = ex.Message });
        }
    }

    // DELETE: api/blocks/5
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

            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bloque eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar bloque");
            return StatusCode(500, new { message = "Error al eliminar bloque", error = ex.Message });
        }
    }
}
