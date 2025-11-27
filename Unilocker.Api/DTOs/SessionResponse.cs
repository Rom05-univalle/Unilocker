namespace Unilocker.Api.DTOs;

public class SessionResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public int ComputerId { get; set; }
    public string ComputerName { get; set; } = null!;
    public string ClassroomName { get; set; } = null!;
    public string BlockName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsActive { get; set; }
    public string? EndMethod { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public int? DurationMinutes { get; set; }
}