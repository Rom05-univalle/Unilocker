namespace Unilocker.Api.DTOs;

public class ClassroomInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int? Capacity { get; set; }
}