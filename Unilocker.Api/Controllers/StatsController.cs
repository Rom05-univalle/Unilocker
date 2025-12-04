using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public StatsController(UnilockerDbContext context)
    {
        _context = context;
    }

    // GET: /api/stats/sessions-by-day?startDate=2025-01-01&endDate=2025-01-31
    [HttpGet("sessions-by-day")]
    public async Task<IActionResult> GetSessionsByDay(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Sessions.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartDateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(s => s.StartDateTime < inclusiveEnd);
        }

        // Últimos 7 días por defecto si no hay filtros
        if (!startDate.HasValue && !endDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var from = today.AddDays(-6);
            query = query.Where(s => s.StartDateTime.Date >= from && s.StartDateTime.Date <= today);
        }

        var result = await query
            .GroupBy(s => s.StartDateTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                count = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    // GET: /api/stats/reports-by-problem?startDate=...&endDate=...
    [HttpGet("reports-by-problem")]
    public async Task<IActionResult> GetReportsByProblem(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Reports.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.CreatedAt < inclusiveEnd);
        }

        var result = await query
            .GroupBy(r => r.ProblemType)
            .OrderByDescending(g => g.Count())
            .Select(g => new
            {
                problemType = g.Key,
                count = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    // GET: /api/stats/top-computers?startDate=...&endDate=...
    [HttpGet("top-computers")]
    public async Task<IActionResult> GetTopComputers(DateTime? startDate, DateTime? endDate, int take = 5)
    {
        var query = _context.Reports
            .Include(r => r.ProblemType)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ReportDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.ReportDate < inclusiveEnd);
        }

        var result = await query
            .GroupBy(r => r.ProblemType.Name)
            .OrderByDescending(g => g.Count())
            .Take(take)
            .Select(g => new
            {
                computerName = g.Key,      // aquí en realidad es el tipo de problema
                reportCount = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }


    // GET: /api/stats/top-users?startDate=...&endDate=...
    [HttpGet("top-users")]
    public async Task<IActionResult> GetTopUsers(DateTime? startDate, DateTime? endDate, int take = 5)
    {
        var query = _context.Sessions
            .Include(s => s.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartDateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(s => s.StartDateTime < inclusiveEnd);
        }

        // Solo sesiones cerradas para calcular minutos
        query = query.Where(s => s.EndDateTime != null);

        var result = await query
            .GroupBy(s => new
            {
                s.UserId,
                s.User.Username,
                s.User.FirstName,
                s.User.LastName,
                s.User.SecondLastName
            })
            .Select(g => new
            {
                username = g.Key.Username,
                fullName = string.Join(" ",
                    new[]
                    {
                        g.Key.FirstName,
                        g.Key.LastName,
                        g.Key.SecondLastName
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),
                totalMinutes = g.Sum(s =>
                    EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime!.Value)),
                sessionCount = g.Count()
            })
            .OrderByDescending(x => x.totalMinutes)
            .ThenByDescending(x => x.sessionCount)
            .Take(take)
            .ToListAsync();

        return Ok(result);
    }

    // GET: /api/stats/average-session-duration?startDate=...&endDate=...
    [HttpGet("average-session-duration")]
    public async Task<IActionResult> GetAverageSessionDuration(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Sessions.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartDateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEnd = endDate.Value.Date.AddDays(1);
            query = query.Where(s => s.StartDateTime < inclusiveEnd);
        }

        query = query.Where(s => s.EndDateTime != null);

        var hasSessions = await query.AnyAsync();
        if (!hasSessions)
        {
            return Ok(new { averageMinutes = 0.0 });
        }

        var avg = await query.AverageAsync(s =>
            (double)EF.Functions.DateDiffMinute(s.StartDateTime, s.EndDateTime!.Value));

        return Ok(new { averageMinutes = avg });
    }
}
