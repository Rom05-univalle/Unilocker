namespace Unilocker.Api.Models;

public class ReportDto
{
    public int Id { get; set; }
    public string User { get; set; } = string.Empty;
    public string Computer { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Pending";
}
