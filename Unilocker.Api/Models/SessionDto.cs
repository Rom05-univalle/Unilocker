namespace Unilocker.Api.Models;

/// <summary>
/// DTO simplificado para listar sesiones en el frontend web
/// </summary>
public class SessionDto
{
    public int Id { get; set; }
    public string User { get; set; } = string.Empty;
    public string Computer { get; set; } = string.Empty;
    public string Classroom { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public string Status { get; set; } = "Activa";
}
