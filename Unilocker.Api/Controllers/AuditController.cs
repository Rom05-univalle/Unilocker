using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/audit
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly UnilockerDbContext _context;

    public AuditController(UnilockerDbContext context)
    {
        _context = context;
    }

    // DTOs para respuesta
    public class AuditLogDto
    {
        public long Id { get; set; }                 // AuditLog.Id
        public string AffectedTable { get; set; } = null!;
        public int RecordId { get; set; }
        public string ActionType { get; set; } = null!;
        public int? ResponsibleUserId { get; set; }
        public string? ResponsibleUserName { get; set; }   // desde ResponsibleUser.Username si existe
        public DateTime ActionDate { get; set; }
        public string? ChangeDetails { get; set; }
        public string? IpAddress { get; set; }
    }

    public class AuditPagedResultDto
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    // GET: /api/audit
    // Filtros: tabla, tipo de acción, usuario y rango de fechas
    [HttpGet]
    public async Task<ActionResult<AuditPagedResultDto>> Get(
        string? table,          // AffectedTable
        string? actionType,     // ActionType (Create, Update, Delete, etc.)
        string? user,           // ResponsibleUser.Username
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var query = _context.AuditLogs
            .Include(a => a.ResponsibleUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(table))
        {
            query = query.Where(a => a.AffectedTable.Contains(table));
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            query = query.Where(a => a.ActionType.Contains(actionType));
        }

        if (!string.IsNullOrWhiteSpace(user))
        {
            query = query.Where(a =>
                a.ResponsibleUser != null &&
                a.ResponsibleUser.Username.Contains(user));
        }

        if (from.HasValue)
        {
            query = query.Where(a => a.ActionDate >= from.Value);
        }

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(a => a.ActionDate < end);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.ActionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                AffectedTable = a.AffectedTable,
                RecordId = a.RecordId,
                ActionType = a.ActionType,
                ResponsibleUserId = a.ResponsibleUserId,
                ResponsibleUserName = a.ResponsibleUser != null ? a.ResponsibleUser.Username : null,
                ActionDate = a.ActionDate,
                ChangeDetails = a.ChangeDetails,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        var result = new AuditPagedResultDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }
}
