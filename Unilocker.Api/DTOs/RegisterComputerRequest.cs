using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class RegisterComputerRequest
{
    [Required(ErrorMessage = "El nombre del equipo es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El UUID es requerido")]
    public Guid Uuid { get; set; }

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(50)]
    public string? OperatingSystem { get; set; }

    [Required(ErrorMessage = "El ID del aula es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del aula debe ser mayor a 0")]
    public int ClassroomId { get; set; }
}