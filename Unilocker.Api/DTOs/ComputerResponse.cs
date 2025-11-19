namespace Unilocker.Api.DTOs;

public class ComputerResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid Uuid { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public bool IsNewRegistration { get; set; }
    public ClassroomInfo? ClassroomInfo { get; set; }
    public DateTime CreatedAt { get; set; }
}