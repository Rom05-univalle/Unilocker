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
            var query = _context.AuditLogs
                .Include(a => a.ResponsibleUser)
                .AsQueryable();

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
                    a.Id,
                    Action = a.ActionType,
                    TableName = a.AffectedTable,
                    a.RecordId,
                    Details = a.ChangeDetails,
                    a.ResponsibleUserId,
                    ResponsibleUserName = a.ResponsibleUser != null 
                        ? a.ResponsibleUser.FirstName + " " + a.ResponsibleUser.LastName
                        : "Sistema",
                    Timestamp = a.ActionDate,
                    a.IpAddress
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
