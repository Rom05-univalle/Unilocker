using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<StatsController> _logger;

    public StatsController(UnilockerDbContext context, ILogger<StatsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/stats/summary
    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetSummary()
    {
        try
        {
            _logger.LogInformation("Obteniendo resumen de estadísticas");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Count de sesiones de hoy (solo computadoras activas)
            var totalSessionsToday = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.StartDateTime >= today && s.StartDateTime < tomorrow && s.Computer.Status)
                .CountAsync();

            // Count de sesiones activas (solo computadoras registradas)
            var activeSessions = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.IsActive && s.Computer.Status)
                .CountAsync();

            // Count de reportes pendientes (solo de computadoras activas)
            var pendingReports = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                .Where(r => r.ReportStatus == "Pending" && r.Session.Computer.Status)
                .CountAsync();

            // Count total de computadoras
            var totalComputers = await _context.Computers.CountAsync();

            var summary = new
            {
                totalSessionsToday,
                activeSessions,
                pendingReports,
                totalComputers,
                generatedAt = DateTime.Now
            };

            _logger.LogInformation(
                "Estadísticas: Sesiones hoy={SessionsToday}, Activas={Active}, Reportes pendientes={Pending}, Computadoras={Computers}",
                totalSessionsToday, activeSessions, pendingReports, totalComputers);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de estadísticas");
            return StatusCode(500, new { message = "Error interno al obtener estadísticas", error = ex.Message });
        }
    }

    // GET: api/stats/top-reported-computers
    [HttpGet("top-reported-computers")]
    public async Task<ActionResult<object>> GetTopReportedComputers([FromQuery] int limit = 5)
    {
        try
        {
            _logger.LogInformation("Obteniendo top {Limit} computadoras con más reportes", limit);

            var topComputers = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                .Where(r => r.Session.Computer.Status == true)
                .GroupBy(r => new
                {
                    ComputerId = r.Session.Computer.Id,
                    ComputerName = r.Session.Computer.Name,
                    ClassroomName = r.Session.Computer.Classroom.Name
                })
                .Select(g => new
                {
                    computerId = g.Key.ComputerId,
                    computerName = g.Key.ComputerName,
                    classroomName = g.Key.ClassroomName,
                    reportCount = g.Count(),
                    pendingReports = g.Count(r => r.ReportStatus == "Pending"),
                    resolvedReports = g.Count(r => r.ReportStatus == "Resolved")
                })
                .OrderByDescending(x => x.reportCount)
                .Take(limit)
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} computadoras con reportes", topComputers.Count);

            return Ok(new
            {
                topComputers,
                limit,
                generatedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top computadoras reportadas");
            return StatusCode(500, new { message = "Error interno al obtener top computadoras", error = ex.Message });
        }
    }

    // GET: api/stats/sessions-by-date
    [HttpGet("sessions-by-date")]
    public async Task<ActionResult<object>> GetSessionsByDate([FromQuery] int days = 7)
    {
        try
        {
            _logger.LogInformation("Obteniendo estadísticas de sesiones de los últimos {Days} días", days);

            var startDate = DateTime.Today.AddDays(-days + 1);

            var sessionsByDate = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.StartDateTime >= startDate && s.Computer.Status)
                .GroupBy(s => s.StartDateTime.Date)
                .Select(g => new
                {
                    date = g.Key,
                    totalSessions = g.Count(),
                    uniqueUsers = g.Select(s => s.UserId).Distinct().Count(),
                    uniqueComputers = g.Select(s => s.ComputerId).Distinct().Count()
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} días con sesiones", sessionsByDate.Count);

            return Ok(new
            {
                sessionsByDate,
                daysRequested = days,
                startDate,
                endDate = DateTime.Today,
                generatedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesiones por fecha");
            return StatusCode(500, new { message = "Error interno al obtener sesiones por fecha", error = ex.Message });
        }
    }

    // GET: api/stats/reports-by-status
    [HttpGet("reports-by-status")]
    public async Task<ActionResult<object>> GetReportsByStatus()
    {
        try
        {
            _logger.LogInformation("Obteniendo distribución de reportes por estado");

            var reportsByStatus = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                .Where(r => r.Session.Computer.Status == true)
                .GroupBy(r => r.ReportStatus)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count(),
                    percentage = 0.0 // Se calculará después
                })
                .ToListAsync();

            var totalReports = reportsByStatus.Sum(x => x.count);

            var reportsByStatusWithPercentage = reportsByStatus.Select(x => new
            {
                x.status,
                x.count,
                percentage = totalReports > 0 ? Math.Round((double)x.count / totalReports * 100, 2) : 0
            }).ToList();

            _logger.LogInformation("Se encontraron {Count} estados diferentes con reportes", reportsByStatus.Count);

            return Ok(new
            {
                reportsByStatus = reportsByStatusWithPercentage,
                totalReports,
                generatedAt = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes por estado");
            return StatusCode(500, new { message = "Error interno al obtener reportes por estado", error = ex.Message });
        }
    }

    // GET: api/stats/sessions-by-day
    [HttpGet("sessions-by-day")]
    public async Task<ActionResult<object>> GetSessionsByDay([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var sessionsByDay = await _context.Sessions
                .Where(s => s.StartDateTime >= start && s.StartDateTime <= end.AddDays(1))
                .GroupBy(s => s.StartDateTime.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count()
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(sessionsByDay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesiones por día");
            return StatusCode(500, new { message = "Error al obtener sesiones por día", error = ex.Message });
        }
    }

    // GET: api/stats/reports-by-problem
    [HttpGet("reports-by-problem")]
    public async Task<ActionResult<object>> GetReportsByProblem([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var reportsByProblem = await _context.Reports
                .Include(r => r.ProblemType)
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end.AddDays(1))
                .GroupBy(r => r.ProblemType!.Name)
                .Select(g => new
                {
                    problemType = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(reportsByProblem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes por tipo de problema");
            return StatusCode(500, new { message = "Error al obtener reportes por problema", error = ex.Message });
        }
    }

    // GET: api/stats/top-computers
    [HttpGet("top-computers")]
    public async Task<ActionResult<object>> GetTopComputers([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var topComputers = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.StartDateTime >= start && s.StartDateTime <= end.AddDays(1))
                .GroupBy(s => s.Computer!.Name)
                .Select(g => new
                {
                    computerName = g.Key,
                    sessionCount = g.Count()
                })
                .OrderByDescending(x => x.sessionCount)
                .Take(10)
                .ToListAsync();

            return Ok(topComputers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener computadoras más usadas");
            return StatusCode(500, new { message = "Error al obtener computadoras", error = ex.Message });
        }
    }

    // GET: api/stats/top-users
    [HttpGet("top-users")]
    public async Task<ActionResult<object>> GetTopUsers([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var topUsers = await _context.Sessions
                .Include(s => s.User)
                .Where(s => s.StartDateTime >= start && s.StartDateTime <= end.AddDays(1))
                .GroupBy(s => s.User!.Username)
                .Select(g => new
                {
                    username = g.Key,
                    sessionCount = g.Count()
                })
                .OrderByDescending(x => x.sessionCount)
                .Take(10)
                .ToListAsync();

            return Ok(topUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios más activos");
            return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
        }
    }
}