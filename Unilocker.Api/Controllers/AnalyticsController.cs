using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;

namespace Unilocker.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(UnilockerDbContext context, ILogger<AnalyticsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el problema más reportado
    /// </summary>
    [HttpGet("top-problem")]
    public async Task<ActionResult> GetTopProblem()
    {
        try
        {
            var topProblem = await _context.Reports
                .GroupBy(r => new { r.ProblemTypeId, r.ProblemType.Name })
                .Select(g => new
                {
                    ProblemTypeId = g.Key.ProblemTypeId,
                    ProblemName = g.Key.Name,
                    TotalReports = g.Count(),
                    PendingReports = g.Count(r => r.ReportStatus == "Pending" || r.ReportStatus == "InReview"),
                    SolvedReports = g.Count(r => r.ReportStatus == "Resolved")
                })
                .OrderByDescending(x => x.TotalReports)
                .FirstOrDefaultAsync();

            if (topProblem == null)
            {
                return Ok(new
                {
                    problemName = "Sin reportes",
                    totalReports = 0,
                    pendingReports = 0,
                    solvedReports = 0
                });
            }

            return Ok(new
            {
                problemName = topProblem.ProblemName,
                totalReports = topProblem.TotalReports,
                pendingReports = topProblem.PendingReports,
                solvedReports = topProblem.SolvedReports
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener problema más reportado");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene el equipo con más fallas en el año actual
    /// </summary>
    [HttpGet("top-failing-computer")]
    public async Task<ActionResult> GetTopFailingComputer()
    {
        try
        {
            var currentYear = DateTime.Now.Year;

            var topComputer = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                .Where(r => r.CreatedAt.Year == currentYear)
                .GroupBy(r => new
                {
                    r.Session.ComputerId,
                    r.Session.Computer.Name,
                    ClassroomName = r.Session.Computer.Classroom.Name
                })
                .Select(g => new
                {
                    ComputerId = g.Key.ComputerId,
                    ComputerName = g.Key.Name,
                    ClassroomName = g.Key.ClassroomName,
                    TotalFailures = g.Count(),
                    PendingFailures = g.Count(r => r.ReportStatus == "Pending" || r.ReportStatus == "InReview")
                })
                .OrderByDescending(x => x.TotalFailures)
                .FirstOrDefaultAsync();

            if (topComputer == null)
            {
                return Ok(new
                {
                    computerName = "Sin reportes",
                    classroomName = "N/A",
                    totalFailures = 0,
                    pendingFailures = 0
                });
            }

            return Ok(new
            {
                computerName = topComputer.ComputerName,
                classroomName = topComputer.ClassroomName,
                totalFailures = topComputer.TotalFailures,
                pendingFailures = topComputer.PendingFailures
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener equipo con más fallas");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene el laboratorio con más fallas en el año actual
    /// </summary>
    [HttpGet("top-failing-classroom")]
    public async Task<ActionResult> GetTopFailingClassroom()
    {
        try
        {
            var currentYear = DateTime.Now.Year;

            var topClassroom = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .Where(r => r.CreatedAt.Year == currentYear)
                .GroupBy(r => new
                {
                    ClassroomId = r.Session.Computer.ClassroomId,
                    ClassroomName = r.Session.Computer.Classroom.Name,
                    BlockName = r.Session.Computer.Classroom.Block.Name,
                    BranchName = r.Session.Computer.Classroom.Block.Branch.Name
                })
                .Select(g => new
                {
                    ClassroomId = g.Key.ClassroomId,
                    ClassroomName = g.Key.ClassroomName,
                    BlockName = g.Key.BlockName,
                    BranchName = g.Key.BranchName,
                    TotalFailures = g.Count(),
                    PendingFailures = g.Count(r => r.ReportStatus == "Pending" || r.ReportStatus == "InReview"),
                    AffectedComputers = g.Select(r => r.Session.ComputerId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalFailures)
                .FirstOrDefaultAsync();

            if (topClassroom == null)
            {
                return Ok(new
                {
                    classroomName = "Sin reportes",
                    blockName = "N/A",
                    branchName = "N/A",
                    totalFailures = 0,
                    pendingFailures = 0,
                    affectedComputers = 0
                });
            }

            return Ok(new
            {
                classroomName = topClassroom.ClassroomName,
                blockName = topClassroom.BlockName,
                branchName = topClassroom.BranchName,
                totalFailures = topClassroom.TotalFailures,
                pendingFailures = topClassroom.PendingFailures,
                affectedComputers = topClassroom.AffectedComputers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener laboratorio con más fallas");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene el tiempo promedio de uso por computadora
    /// </summary>
    [HttpGet("average-usage-by-computer")]
    public async Task<ActionResult> GetAverageUsageByComputer()
    {
        try
        {
            var averageUsage = await _context.Sessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Classroom)
                .Where(s => s.IsActive == false && s.StartDateTime != null && s.EndDateTime != null)
                .GroupBy(s => new
                {
                    s.ComputerId,
                    ComputerName = s.Computer.Name,
                    ClassroomName = s.Computer.Classroom.Name
                })
                .Select(g => new
                {
                    ComputerId = g.Key.ComputerId,
                    ComputerName = g.Key.ComputerName,
                    ClassroomName = g.Key.ClassroomName,
                    TotalSessions = g.Count(),
                    AverageMinutes = g.Average(s => EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime)),
                    TotalHours = g.Sum(s => EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime)) / 60.0
                })
                .OrderByDescending(x => x.AverageMinutes)
                .Take(10)
                .ToListAsync();

            return Ok(averageUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener promedio de uso por computadora");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene el tiempo promedio de uso por laboratorio
    /// </summary>
    [HttpGet("average-usage-by-classroom")]
    public async Task<ActionResult> GetAverageUsageByClassroom()
    {
        try
        {
            var averageUsage = await _context.Sessions
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Classroom)
                        .ThenInclude(cl => cl.Block)
                            .ThenInclude(b => b.Branch)
                .Where(s => s.IsActive == false && s.StartDateTime != null && s.EndDateTime != null)
                .GroupBy(s => new
                {
                    ClassroomId = s.Computer.ClassroomId,
                    ClassroomName = s.Computer.Classroom.Name,
                    BlockName = s.Computer.Classroom.Block.Name,
                    BranchName = s.Computer.Classroom.Block.Branch.Name
                })
                .Select(g => new
                {
                    ClassroomId = g.Key.ClassroomId,
                    ClassroomName = g.Key.ClassroomName,
                    BlockName = g.Key.BlockName,
                    BranchName = g.Key.BranchName,
                    TotalSessions = g.Count(),
                    TotalComputers = g.Select(s => s.ComputerId).Distinct().Count(),
                    AverageMinutes = g.Average(s => EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime)),
                    TotalHours = g.Sum(s => EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime)) / 60.0
                })
                .OrderByDescending(x => x.TotalHours)
                .Take(10)
                .ToListAsync();

            return Ok(averageUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener promedio de uso por laboratorio");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene las horas pico de utilización
    /// </summary>
    [HttpGet("peak-hours")]
    public async Task<ActionResult> GetPeakHours()
    {
        try
        {
            var peakHours = await _context.Sessions
                .Where(s => s.StartDateTime != null)
                .GroupBy(s => s.StartDateTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TotalSessions = g.Count(),
                    ActiveSessions = g.Count(s => s.IsActive == true)
                })
                .OrderByDescending(x => x.TotalSessions)
                .ToListAsync();

            return Ok(peakHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener horas pico");
            return StatusCode(500, new { error = "Error al obtener estadísticas" });
        }
    }
}
