using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class StartSessionRequest
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El ID de la computadora es requerido")]
    public int ComputerId { get; set; }
}