using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProblemTypesController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<ProblemTypesController> _logger;

    public ProblemTypesController(UnilockerDbContext context, ILogger<ProblemTypesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/problemtypes
    [HttpGet]
    public async Task<ActionResult<object>> GetProblemTypes([FromQuery] bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Obteniendo tipos de problema (activeOnly={ActiveOnly})", activeOnly);

            var query = _context.ProblemTypes.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(pt => pt.Status);
            }

            var problemTypes = await query
                .OrderBy(pt => pt.Name)
                .Select(pt => new
                {
                    pt.Id,
                    pt.Name,
                    pt.Description,
                    pt.Status
                })
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} tipos de problema", problemTypes.Count);

            return Ok(problemTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de problema");
            return StatusCode(500, new { message = "Error interno al obtener tipos de problema", error = ex.Message });
        }
    }

    // GET: api/problemtypes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProblemTypeById(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo tipo de problema con ID: {ProblemTypeId}", id);

            var problemType = await _context.ProblemTypes
                .Where(pt => pt.Id == id)
                .Select(pt => new
                {
                    pt.Id,
                    pt.Name,
                    pt.Description,
                    pt.Status,
                    reportCount = pt.Reports.Count,
                    pendingReports = pt.Reports.Count(r => r.ReportStatus == "Pending")
                })
                .FirstOrDefaultAsync();

            if (problemType == null)
            {
                _logger.LogWarning("Tipo de problema no encontrado: {ProblemTypeId}", id);
                return NotFound(new { message = $"Tipo de problema con ID {id} no encontrado" });
            }

            return Ok(problemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipo de problema {ProblemTypeId}", id);
            return StatusCode(500, new { message = "Error interno al obtener tipo de problema", error = ex.Message });
        }
    }

    // GET: api/problemtypes/stats
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetProblemTypeStats()
    {
        try
        {
            _logger.LogInformation("Obteniendo estadísticas de tipos de problema");

            var stats = await _context.ProblemTypes
                .Where(pt => pt.Status)
                .Select(pt => new
                {
                    pt.Id,
                    pt.Name,
                    totalReports = pt.Reports.Count,
                    pendingReports = pt.Reports.Count(r => r.ReportStatus == "Pending"),
                    resolvedReports = pt.Reports.Count(r => r.ReportStatus == "Resolved"),
                    rejectedReports = pt.Reports.Count(r => r.ReportStatus == "Rejected")
                })
                .OrderByDescending(x => x.totalReports)
                .ToListAsync();

            var totalReports = stats.Sum(x => x.totalReports);

            _logger.LogInformation("Se generaron estadísticas para {Count} tipos de problema", stats.Count);

            return Ok(new
            {
                problemTypeStats = stats,
                totalReports,
                totalTypes = stats.Count,
                generatedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de tipos de problema");
            return StatusCode(500, new { message = "Error interno al obtener estadísticas", error = ex.Message });
        }
    }

    // POST: api/problemtypes
    [HttpPost]
    public async Task<ActionResult> CreateProblemType([FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("CreateProblemType - Payload: {Payload}", dto.ToString());

            var name = dto.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : string.Empty;
            var description = dto.TryGetProperty("description", out var descEl) && descEl.ValueKind != System.Text.Json.JsonValueKind.Null
                ? descEl.GetString() : null;
            var status = dto.TryGetProperty("status", out var statusEl) ? statusEl.GetBoolean() : true;

            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { message = "El nombre es obligatorio" });
            }

            var problemType = new Models.ProblemType
            {
                Name = name,
                Description = description,
                Status = status,
                CreatedAt = DateTime.Now
            };

            _context.ProblemTypes.Add(problemType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de problema creado exitosamente: {ProblemTypeId}", problemType.Id);
            return CreatedAtAction(nameof(GetProblemTypeById), new { id = problemType.Id }, problemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de problema");
            return StatusCode(500, new { message = "Error al crear tipo de problema", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // PUT: api/problemtypes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProblemType(int id, [FromBody] System.Text.Json.JsonElement dto)
    {
        try
        {
            _logger.LogInformation("UpdateProblemType - ID: {Id}, Payload: {Payload}", id, dto.ToString());

            var existingProblemType = await _context.ProblemTypes.FindAsync(id);
            if (existingProblemType == null)
            {
                return NotFound(new { message = "Tipo de problema no encontrado" });
            }

            if (dto.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                existingProblemType.Name = nameEl.GetString() ?? existingProblemType.Name;
            }
            if (dto.TryGetProperty("description", out var descEl))
            {
                existingProblemType.Description = descEl.ValueKind != System.Text.Json.JsonValueKind.Null ? descEl.GetString() : null;
            }
            if (dto.TryGetProperty("status", out var statusEl))
            {
                existingProblemType.Status = statusEl.GetBoolean();
            }
            existingProblemType.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de problema actualizado exitosamente: {ProblemTypeId}", id);
            return Ok(existingProblemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de problema");
            return StatusCode(500, new { message = "Error al actualizar tipo de problema", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // DELETE: api/problemtypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProblemType(int id)
    {
        try
        {
            var problemType = await _context.ProblemTypes.FindAsync(id);
            if (problemType == null)
            {
                return NotFound(new { message = "Tipo de problema no encontrado" });
            }

            _context.ProblemTypes.Remove(problemType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de problema eliminado: {ProblemTypeId}", id);
            return Ok(new { message = "Tipo de problema eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tipo de problema");
            return StatusCode(500, new { message = "Error al eliminar tipo de problema", error = ex.Message });
        }
    }
}