namespace Unilocker.Api.Models;

/// <summary>
/// DTO para tipos de problemas en el frontend web
/// </summary>
public class ProblemTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
