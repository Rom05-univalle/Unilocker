using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(UnilockerDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

   
    [HttpPost]
    public async Task<ActionResult<ReportResponse>> CreateReport([FromBody] CreateReportRequest request)
    {
        try
        {
            _logger.LogInformation("Creando reporte para SessionId: {SessionId}, ProblemTypeId: {ProblemTypeId}",
                request.SessionId, request.ProblemTypeId);

        
            var session = await _context.Sessions
                .Include(s => s.User)
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Classroom)
                        .ThenInclude(cl => cl.Block)
                            .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId);

            if (session == null)
            {
                _logger.LogWarning("Sesión no encontrada: {SessionId}", request.SessionId);
                return NotFound(new { message = $"Sesión con ID {request.SessionId} no encontrada" });
            }

         
            var problemType = await _context.ProblemTypes
                .FirstOrDefaultAsync(pt => pt.Id == request.ProblemTypeId && pt.Status);

            if (problemType == null)
            {
                _logger.LogWarning("Tipo de problema no encontrado o inactivo: {ProblemTypeId}", request.ProblemTypeId);
                return BadRequest(new { message = $"Tipo de problema con ID {request.ProblemTypeId} no encontrado o inactivo" });
            }


            var report = new Report
            {
                SessionId = request.SessionId,
                ProblemTypeId = request.ProblemTypeId,
                Description = request.Description,
                ReportDate = DateTime.Now,
                ReportStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reporte creado exitosamente con ID: {ReportId}", report.Id);


            var createdReport = await _context.Reports
                .Include(r => r.ProblemType)
                .Include(r => r.Session)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .FirstAsync(r => r.Id == report.Id);

            var response = MapToReportResponse(createdReport);

            return CreatedAtAction(nameof(GetReportById), new { id = report.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reporte");
            return StatusCode(500, new { message = "Error interno al crear el reporte", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ReportResponse>>> GetReports(
        [FromQuery] string? status = null,
        [FromQuery] int? problemTypeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Obteniendo reportes con filtros - Status: {Status}, ProblemTypeId: {ProblemTypeId}, StartDate: {StartDate}, EndDate: {EndDate}",
                status, problemTypeId, startDate, endDate);

            var query = _context.Reports
                .Include(r => r.ProblemType)
                .Include(r => r.Session)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.ReportStatus == status);
            }

            if (problemTypeId.HasValue)
            {
                query = query.Where(r => r.ProblemTypeId == problemTypeId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.ReportDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.ReportDate <= endOfDay);
            }


            query = query.OrderByDescending(r => r.ReportDate);


            var totalRecords = await query.CountAsync();
            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = reports.Select(MapToReportResponse).ToList();

            _logger.LogInformation("Se encontraron {Count} reportes de {Total} totales", response.Count, totalRecords);

            Response.Headers.Append("X-Total-Count", totalRecords.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes");
            return StatusCode(500, new { message = "Error interno al obtener reportes", error = ex.Message });
        }
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ReportResponse>> GetReportById(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo reporte con ID: {ReportId}", id);

            var report = await _context.Reports
                .Include(r => r.ProblemType)
                .Include(r => r.Session)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                _logger.LogWarning("Reporte no encontrado: {ReportId}", id);
                return NotFound(new { message = $"Reporte con ID {id} no encontrado" });
            }

            var response = MapToReportResponse(report);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte {ReportId}", id);
            return StatusCode(500, new { message = "Error interno al obtener el reporte", error = ex.Message });
        }
    }


    [HttpPut("{id}/status")]
    public async Task<ActionResult<ReportResponse>> UpdateReportStatus(int id, [FromBody] UpdateReportStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Actualizando estado de reporte {ReportId} a {NewStatus}", id, request.ReportStatus);

            var report = await _context.Reports
                .Include(r => r.ProblemType)
                .Include(r => r.Session)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                _logger.LogWarning("Reporte no encontrado: {ReportId}", id);
                return NotFound(new { message = $"Reporte con ID {id} no encontrado" });
            }


            var validStatuses = new[] { "Pending", "InReview", "Resolved", "Rejected" };
            if (!validStatuses.Contains(request.ReportStatus))
            {
                return BadRequest(new { message = $"Estado '{request.ReportStatus}' no es válido. Estados válidos: {string.Join(", ", validStatuses)}" });
            }


            report.ReportStatus = request.ReportStatus;
            report.UpdatedAt = DateTime.Now;


            if (request.ReportStatus == "Resolved" && !report.ResolutionDate.HasValue)
            {
                report.ResolutionDate = DateTime.Now;
                _logger.LogInformation("Reporte {ReportId} marcado como resuelto", id);
            }

            await _context.SaveChangesAsync();

            var response = MapToReportResponse(report);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de reporte {ReportId}", id);
            return StatusCode(500, new { message = "Error interno al actualizar el estado", error = ex.Message });
        }
    }


    [HttpGet("pending")]
    public async Task<ActionResult<List<ReportResponse>>> GetPendingReports()
    {
        try
        {
            _logger.LogInformation("Obteniendo reportes pendientes");

            var reports = await _context.Reports
                .Include(r => r.ProblemType)
                .Include(r => r.Session)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Computer)
                        .ThenInclude(c => c.Classroom)
                            .ThenInclude(cl => cl.Block)
                                .ThenInclude(b => b.Branch)
                .Where(r => r.ReportStatus == "Pending")
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

            var response = reports.Select(MapToReportResponse).ToList();

            _logger.LogInformation("Se encontraron {Count} reportes pendientes", response.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes pendientes");
            return StatusCode(500, new { message = "Error interno al obtener reportes pendientes", error = ex.Message });
        }
    }

 
    private ReportResponse MapToReportResponse(Report report)
    {
        return new ReportResponse
        {
            Id = report.Id,
            SessionId = report.SessionId,
            ProblemTypeId = report.ProblemTypeId,
            ProblemTypeName = report.ProblemType.Name,
            Description = report.Description,
            ReportDate = report.ReportDate,
            ReportStatus = report.ReportStatus,
            ResolutionDate = report.ResolutionDate,
            UserName = report.Session.User.Username,
            UserFullName = $"{report.Session.User.FirstName} {report.Session.User.LastName}".Trim(),
            ComputerName = report.Session.Computer.Name,
            ClassroomName = report.Session.Computer.Classroom.Name,
            BlockName = report.Session.Computer.Classroom.Block.Name,
            BranchName = report.Session.Computer.Classroom.Block.Branch.Name
        };
    }
}