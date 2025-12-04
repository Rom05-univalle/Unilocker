using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public DashboardController(UnilockerDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today = DateTime.Today;

        // Total de sesiones iniciadas hoy (independiente de si siguen activas)
        var totalSessionsToday = await _context.Sessions
            .CountAsync(s => s.StartDateTime >= today);

        // Sesiones activas
        var activeSessions = await _context.Sessions
            .CountAsync(s => s.IsActive);

        // Reportes pendientes
        var pendingReports = await _context.Reports
            .CountAsync(r => r.ReportStatus == "Pending");

        // Computadoras registradas activas
        var registeredComputers = await _context.Computers
            .CountAsync(c => c.Status);

        var result = new
        {
            totalSessionsToday,
            activeSessions,
            pendingReports,
            registeredComputers
        };

        return Ok(result);
    }
}
