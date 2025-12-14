using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, ErrorMessage = "El username no puede exceder 50 caracteres")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = null!;

    public int? ComputerId { get; set; }
}