namespace Unilocker.Api.DTOs;

public class ReportResponse
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int ProblemTypeId { get; set; }
    public string ProblemTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string ReportStatus { get; set; } = string.Empty;
    public DateTime? ResolutionDate { get; set; }

    // Datos relacionados
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string ClassroomName { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
}