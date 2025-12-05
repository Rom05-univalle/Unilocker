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
}