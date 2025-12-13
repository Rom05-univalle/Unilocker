using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

/// <summary>
/// Controlador para el dashboard web
/// Proporciona estadísticas y datos de resumen
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(UnilockerDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtener estadísticas para el dashboard web con KPIs principales
    /// GET: api/dashboard/stats
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            _logger.LogInformation("Obteniendo estadísticas para dashboard web");

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // KPI 1: Total de infraestructuras (sucursales/branches)
            var totalBranches = await _context.Branches
                .Where(b => b.Status)
                .CountAsync();

            // KPI 2: Total de laboratorios (aulas/classrooms)
            var totalClassrooms = await _context.Classrooms
                .Where(c => c.Status)
                .CountAsync();

            // KPI 3: Total de computadoras registradas
            var totalComputers = await _context.Computers
                .Where(c => c.Status)
                .CountAsync();

            // KPI 4: Computadoras activas (con sesión activa en este momento)
            var activeComputers = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.IsActive && s.Computer.Status)
                .CountAsync();

            // Calcular % de computadoras activas
            var activeComputersPercentage = totalComputers > 0 
                ? Math.Round((double)activeComputers / totalComputers * 100, 2)
                : 0;

            // KPI 5: Horas totales de uso (mes actual)
            var sessionsThisMonth = await _context.Sessions
                .Include(s => s.Computer)
                .Where(s => s.StartDateTime >= firstDayOfMonth 
                         && s.StartDateTime <= lastDayOfMonth 
                         && s.Computer.Status)
                .ToListAsync();

            // Calcular horas totales considerando sesiones cerradas y activas
            double totalHoursThisMonth = 0;
            foreach (var session in sessionsThisMonth)
            {
                if (session.EndDateTime.HasValue)
                {
                    // Sesión cerrada: calcular tiempo real
                    totalHoursThisMonth += (session.EndDateTime.Value - session.StartDateTime).TotalHours;
                }
                else if (session.IsActive)
                {
                    // Sesión activa: calcular tiempo hasta ahora
                    totalHoursThisMonth += (DateTime.Now - session.StartDateTime).TotalHours;
                }
            }
            totalHoursThisMonth = Math.Round(totalHoursThisMonth, 2);

            // KPI 6: Incidencias abiertas (reportes pendientes o en revisión)
            var openIncidents = await _context.Reports
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                .Where(r => (r.ReportStatus == "Pending" || r.ReportStatus == "InReview") 
                         && r.Session.Computer.Status)
                .CountAsync();

            var stats = new
            {
                // KPIs principales
                totalBranches,
                totalClassrooms,
                totalComputers,
                activeComputers,
                activeComputersPercentage,
                totalHoursThisMonth,
                openIncidents,

                // Datos adicionales
                totalUsers = await _context.Users.Where(u => u.Status).CountAsync(),
                totalSessions = await _context.Sessions.CountAsync(),
                currentMonth = today.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))
            };

            _logger.LogInformation(
                "Dashboard KPIs: Sucursales={Branches}, Laboratorios={Labs}, Computadoras={Computers}, " +
                "Activas={Active} ({Percentage}%), Horas mes={Hours}, Incidencias={Incidents}",
                stats.totalBranches, stats.totalClassrooms, stats.totalComputers, 
                stats.activeComputers, stats.activeComputersPercentage, stats.totalHoursThisMonth, stats.openIncidents);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
