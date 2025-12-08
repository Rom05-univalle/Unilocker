namespace Unilocker.Api.DTOs;

public class UpdateComputerRequest
{
    public string? Name { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int? ClassroomId { get; set; }
    public bool? Status { get; set; }
}
