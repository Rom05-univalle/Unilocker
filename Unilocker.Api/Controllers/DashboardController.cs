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
    /// Obtener estadísticas para el dashboard web
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
            var tomorrow = today.AddDays(1);

            // Sesiones de hoy
            var totalSessionsToday = await _context.Sessions
                .Where(s => s.StartDateTime >= today && s.StartDateTime < tomorrow)
                .CountAsync();

            // Sesiones activas
            var activeSessions = await _context.Sessions
                .Where(s => s.IsActive)
                .CountAsync();

            // Reportes pendientes
            var pendingReports = await _context.Reports
                .Where(r => r.ReportStatus == "Pending")
                .CountAsync();

            // Total de computadoras registradas
            var registeredComputers = await _context.Computers
                .Where(c => c.Status)
                .CountAsync();

            var stats = new
            {
                totalSessionsToday,
                activeSessions,
                pendingReports,
                registeredComputers
            };

            _logger.LogInformation(
                "Dashboard stats: Sesiones hoy={Today}, Activas={Active}, Reportes={Reports}, PCs={Computers}",
                totalSessionsToday, activeSessions, pendingReports, registeredComputers);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
