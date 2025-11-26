namespace Unilocker.Api.DTOs;

public class CreateReportRequest
{
    public int SessionId { get; set; }
    public int ProblemTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
}