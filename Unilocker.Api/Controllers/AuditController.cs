using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<AuditController> _logger;

    public AuditController(UnilockerDbContext context, ILogger<AuditController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/audit
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? action,
        [FromQuery] int? userId)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            // Filtrar por fecha de inicio
            if (startDate.HasValue)
            {
                query = query.Where(a => a.ActionDate >= startDate.Value);
            }

            // Filtrar por fecha fin
            if (endDate.HasValue)
            {
                query = query.Where(a => a.ActionDate <= endDate.Value.AddDays(1));
            }

            // Filtrar por acción
            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.ActionType.Contains(action));
            }

            // Filtrar por usuario
            if (userId.HasValue)
            {
                query = query.Where(a => a.ResponsibleUserId == userId.Value);
            }

            var auditLogs = await query
                .OrderByDescending(a => a.ActionDate)
                .Select(a => new
                {
                    Id = a.Id,
                    ActionType = a.ActionType,
                    AffectedTable = a.AffectedTable,
                    RecordId = a.RecordId,
                    ChangeDetails = a.ChangeDetails,
                    ResponsibleUserId = a.ResponsibleUserId,
                    ResponsibleUserName = a.ResponsibleUserId.HasValue 
                        ? _context.Users.Where(u => u.Id == a.ResponsibleUserId.Value).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                        : "Sistema",
                    ActionDate = a.ActionDate,
                    IpAddress = a.IpAddress
                })
                .ToListAsync();

            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs de auditoría");
            return StatusCode(500, new { message = "Error al obtener auditoría", error = ex.Message });
        }
    }
}
