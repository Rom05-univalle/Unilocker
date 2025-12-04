using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

using System.Security.Claims;

namespace Unilocker.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Requiere JWT token para todos los endpoints
public class SessionsController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(UnilockerDbContext context, ILogger<SessionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar una nueva sesión
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando sesión - UserId: {UserId}, ComputerId: {ComputerId}",
                request.UserId, request.ComputerId);

            // 1. Validar que el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status);
            if (!userExists)
            {
                return BadRequest(new { message = "Usuario no encontrado o inactivo" });
            }

            // 2. Validar que la computadora existe
            var computerExists = await _context.Computers.AnyAsync(c => c.Id == request.ComputerId && c.Status);
            if (!computerExists)
            {
                return BadRequest(new { message = "Computadora no encontrada o inactiva" });
            }

            // 3. Verificar si el usuario ya tiene una sesión activa
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.IsActive);

            if (activeSession != null)
            {
                return Conflict(new
                {
                    message = "El usuario ya tiene una sesión activa",
                    activeSessionId = activeSession.Id
                });
            }

            // 4. Crear nueva sesión
            var session = new Session
            {
                UserId = request.UserId,
                ComputerId = request.ComputerId,
                StartDateTime = DateTime.Now,
                IsActive = true,
                LastHeartbeat = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sesión creada exitosamente - SessionId: {SessionId}", session.Id);

            // 5. Cargar datos relacionados y retornar
            var sessionResponse = await GetSessionResponseById(session.Id);

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, sessionResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Finalizar una sesión
    /// </summary>
    [HttpPut("{id}/end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionResponse>> EndSession(int id, [FromBody] EndSessionRequest request)
    {
        try
        {
            _logger.LogInformation("Finalizando sesión {SessionId} con método: {EndMethod}",
                id, request.EndMethod);

            // 1. Buscar sesión
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound(new { message = "Sesión no encontrada" });
            }

            // 2. Validar que esté activa
            if (!session.IsActive)
            {
                return BadRequest(new { message = "La sesión ya fue finalizada" });
            }

            // 3. Actualizar sesión
            session.EndDateTime = DateTime.Now;
            session.IsActive = false;
            session.EndMethod = request.EndMethod;
            session.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sesión {SessionId} finalizada exitosamente", id);

            // 4. Retornar sesión actualizada
            var sessionResponse = await GetSessionResponseById(id);
            return Ok(sessionResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar sesión {SessionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar heartbeat de la sesión
    /// </summary>
    [HttpPost("{id}/heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Heartbeat(int id)
    {
        try
        {
            _logger.LogInformation("Heartbeat recibido - SessionId: {SessionId}", id);

            // 1. Buscar sesión activa
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (session == null)
            {
                return NotFound(new { message = "Sesión no encontrada o inactiva" });
            }

            // 2. Actualizar LastHeartbeat
            session.LastHeartbeat = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Heartbeat actualizado",
                sessionId = id,
                lastHeartbeat = session.LastHeartbeat
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar heartbeat - SessionId: {SessionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener una sesión por id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> GetSession(int id)
    {
        try
        {
            var sessionResponse = await GetSessionResponseById(id);
            if (sessionResponse == null)
            {
                return NotFound(new { message = "Sesión no encontrada" });
            }

            return Ok(sessionResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesión {SessionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todas las sesiones activas
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> GetActiveSessions()
    {
        try
        {
            var sessions = await _context.Sessions
                .Where(s => s.IsActive)
                .Include(s => s.User)
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Classroom)
                    .ThenInclude(cl => cl.Block)
                    .ThenInclude(b => b.Branch)
                .OrderByDescending(s => s.StartDateTime)
                .ToListAsync();

            var response = sessions.Select(s => MapToSessionResponse(s)).ToList();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesiones activas");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener sesiones (paginadas, filtradas).
    /// Ahora soporta userId=me para el usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> GetSessions(
        [FromQuery] string? userId = null,
        [FromQuery] int? computerId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Sessions
                .Include(s => s.User)
                .Include(s => s.Computer)
                    .ThenInclude(c => c.Classroom)
                    .ThenInclude(cl => cl.Block)
                    .ThenInclude(b => b.Branch)
                .AsQueryable();

            // Soporte userId=me (para perfil)
            int? effectiveUserId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                if (string.Equals(userId, "me", StringComparison.OrdinalIgnoreCase))
                {
                    var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out var parsedId))
                    {
                        return Unauthorized();
                    }
                    effectiveUserId = parsedId;
                }
                else if (int.TryParse(userId, out var parsedUserId))
                {
                    effectiveUserId = parsedUserId;
                }
            }

            if (effectiveUserId.HasValue)
            {
                query = query.Where(s => s.UserId == effectiveUserId.Value);
            }

            if (computerId.HasValue)
            {
                query = query.Where(s => s.ComputerId == computerId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var sessions = await query
                .OrderByDescending(s => s.StartDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = sessions.Select(s => MapToSessionResponse(s)).ToList();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesiones");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // =========================
    // Métodos privados de ayuda
    // =========================

    private async Task<SessionResponse?> GetSessionResponseById(int id)
    {
        var session = await _context.Sessions
            .Include(s => s.User)
            .Include(s => s.Computer)
                .ThenInclude(c => c.Classroom)
                .ThenInclude(cl => cl.Block)
                .ThenInclude(b => b.Branch)
            .FirstOrDefaultAsync(s => s.Id == id);

        return session != null ? MapToSessionResponse(session) : null;
    }

    private SessionResponse MapToSessionResponse(Session session)
    {
        int? durationMinutes = null;
        if (session.EndDateTime.HasValue)
        {
            durationMinutes = (int)(session.EndDateTime.Value - session.StartDateTime).TotalMinutes;
        }
        else if (session.IsActive)
        {
            durationMinutes = (int)(DateTime.Now - session.StartDateTime).TotalMinutes;
        }

        return new SessionResponse
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = session.User.Username,
            UserFullName = $"{session.User.FirstName} {session.User.LastName}",
            ComputerId = session.ComputerId,
            ComputerName = session.Computer.Name ?? "Sin nombre",
            ClassroomName = session.Computer.Classroom?.Name ?? "Sin aula",
            BlockName = session.Computer.Classroom?.Block?.Name ?? "Sin bloque",
            BranchName = session.Computer.Classroom?.Block?.Branch?.Name ?? "Sin sede",
            StartDateTime = session.StartDateTime,
            EndDateTime = session.EndDateTime,
            IsActive = session.IsActive,
            EndMethod = session.EndMethod,
            LastHeartbeat = session.LastHeartbeat,
            DurationMinutes = durationMinutes
        };
    }
}
