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
        [FromQuery] string? table,
        [FromQuery] string? actionType,
        [FromQuery] string? user,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            // Filtrar por tabla afectada
            if (!string.IsNullOrEmpty(table))
            {
                query = query.Where(a => a.AffectedTable.Contains(table));
            }

            // Filtrar por tipo de acción
            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(a => a.ActionType.Contains(actionType));
            }

            // Filtrar por nombre de usuario
            if (!string.IsNullOrEmpty(user))
            {
                var userIds = await _context.Users
                    .Where(u => (u.FirstName + " " + u.LastName).Contains(user) || u.Username.Contains(user))
                    .Select(u => u.Id)
                    .ToListAsync();
                query = query.Where(a => a.ResponsibleUserId.HasValue && userIds.Contains(a.ResponsibleUserId.Value));
            }

            // Filtrar por fecha desde
            if (from.HasValue)
            {
                query = query.Where(a => a.ActionDate >= from.Value);
            }

            // Filtrar por fecha hasta
            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.ActionDate <= toDate);
            }

            // Contar total antes de paginar
            var total = await query.CountAsync();

            // Paginar
            var auditLogs = await query
                .OrderByDescending(a => a.ActionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return Ok(new
            {
                items = auditLogs,
                total,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs de auditoría");
            return StatusCode(500, new { message = "Error al obtener auditoría", error = ex.Message });
        }
    }
}
