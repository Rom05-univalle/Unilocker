using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.DTOs;
using Unilocker.Api.Models;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComputersController : ControllerBase
{
    private readonly UnilockerDbContext _context;
    private readonly ILogger<ComputersController> _logger;

    public ComputersController(UnilockerDbContext context, ILogger<ComputersController> logger)
    {
        _context = context;
        _logger = logger;
    }

   
    [HttpPost("register")]
    [ProducesResponseType(typeof(ComputerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ComputerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComputerResponse>> RegisterComputer([FromBody] RegisterComputerRequest request)
    {
        try
        {
            _logger.LogInformation("Intentando registrar computadora con UUID: {Uuid}", request.Uuid);

            // 1. Buscar si la computadora ya existe por UUID
            var existingComputer = await _context.Computers
                .Include(c => c.Classroom)
                    .ThenInclude(cl => cl.Block)
                        .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(c => c.Uuid == request.Uuid);

            // 2. Si existe, retornar la información existente
            if (existingComputer != null)
            {
                _logger.LogInformation("Computadora ya registrada con ID: {Id}", existingComputer.Id);

                var existingResponse = new ComputerResponse
                {
                    Id = existingComputer.Id,
                    Name = existingComputer.Name,
                    Uuid = existingComputer.Uuid,
                    Model = existingComputer.Model,
                    SerialNumber = existingComputer.SerialNumber,
                    IsNewRegistration = false,
                    CreatedAt = existingComputer.CreatedAt,
                    ClassroomInfo = existingComputer.Classroom != null ? new ClassroomInfo
                    {
                        Id = existingComputer.Classroom.Id,
                        Name = existingComputer.Classroom.Name,
                        BlockName = existingComputer.Classroom.Block?.Name ?? "N/A",
                        BranchName = existingComputer.Classroom.Block?.Branch?.Name ?? "N/A",
                        Capacity = existingComputer.Classroom.Capacity
                    } : null
                };

                return Ok(existingResponse);
            }

            // 3. Validar que el aula existe y está activa
            var classroom = await _context.Classrooms
                .Include(c => c.Block)
                    .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.Status == true);

            if (classroom == null)
            {
                _logger.LogWarning("Aula no encontrada o inactiva: {ClassroomId}", request.ClassroomId);
                return BadRequest(new { error = "El aula especificada no existe o está inactiva" });
            }

            // 4. Crear nueva computadora
            var newComputer = new Computer
            {
                Name = request.Name,
                Uuid = request.Uuid,
                SerialNumber = request.SerialNumber,
                Model = request.Model,
                ClassroomId = request.ClassroomId,
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Computers.Add(newComputer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nueva computadora registrada con ID: {Id}", newComputer.Id);

            // 5. Retornar respuesta de éxito
            var response = new ComputerResponse
            {
                Id = newComputer.Id,
                Name = newComputer.Name,
                Uuid = newComputer.Uuid,
                Model = newComputer.Model,
                SerialNumber = newComputer.SerialNumber,
                IsNewRegistration = true,
                CreatedAt = newComputer.CreatedAt,
                ClassroomInfo = new ClassroomInfo
                {
                    Id = classroom.Id,
                    Name = classroom.Name,
                    BlockName = classroom.Block?.Name ?? "N/A",
                    BranchName = classroom.Block?.Branch?.Name ?? "N/A",
                    Capacity = classroom.Capacity
                }
            };

            return CreatedAtAction(nameof(GetComputerById), new { id = newComputer.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar computadora");
            return StatusCode(500, new { error = "Error interno al registrar la computadora", details = ex.Message });
        }
    }

   
    [HttpGet("classrooms")]
    [ProducesResponseType(typeof(IEnumerable<ClassroomInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClassroomInfo>>> GetClassrooms()
    {
        try
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.Block)
                    .ThenInclude(b => b.Branch)
                .Where(c => c.Status == true)
                .OrderBy(c => c.Block.Branch.Name)
                .ThenBy(c => c.Block.Name)
                .ThenBy(c => c.Name)
                .Select(c => new ClassroomInfo
                {
                    Id = c.Id,
                    Name = c.Name,
                    BlockName = c.Block.Name,
                    BranchName = c.Block.Branch.Name,
                    Capacity = c.Capacity
                })
                .ToListAsync();

            _logger.LogInformation("Se obtuvieron {Count} aulas", classrooms.Count);
            return Ok(classrooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener aulas");
            return StatusCode(500, new { error = "Error al obtener la lista de aulas" });
        }
    }

   
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ComputerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComputerResponse>> GetComputerById(int id)
    {
        try
        {
            var computer = await _context.Computers
                .Include(c => c.Classroom)
                    .ThenInclude(cl => cl.Block)
                        .ThenInclude(b => b.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                _logger.LogWarning("Computadora no encontrada: {Id}", id);
                return NotFound(new { error = "Computadora no encontrada" });
            }

            var response = new ComputerResponse
            {
                Id = computer.Id,
                Name = computer.Name,
                Uuid = computer.Uuid,
                Model = computer.Model,
                SerialNumber = computer.SerialNumber,
                IsNewRegistration = false,
                CreatedAt = computer.CreatedAt,
                ClassroomInfo = computer.Classroom != null ? new ClassroomInfo
                {
                    Id = computer.Classroom.Id,
                    Name = computer.Classroom.Name,
                    BlockName = computer.Classroom.Block?.Name ?? "N/A",
                    BranchName = computer.Classroom.Block?.Branch?.Name ?? "N/A",
                    Capacity = computer.Classroom.Capacity
                } : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener computadora {Id}", id);
            return StatusCode(500, new { error = "Error al obtener la computadora" });
        }
    }
}