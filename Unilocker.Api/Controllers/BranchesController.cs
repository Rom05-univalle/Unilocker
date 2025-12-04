using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
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
                    BlockCount = b.Blocks.Count
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

            existingBranch.Name = branch.Name;
            existingBranch.Address = branch.Address;
            existingBranch.Code = branch.Code;
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

    // DELETE: api/branches/5
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

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sede eliminada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar sede");
            return StatusCode(500, new { message = "Error al eliminar sede", error = ex.Message });
        }
    }
}
