using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models;

public class SessionResponse
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int ComputerId { get; set; }
    public string ComputerName { get; set; } = string.Empty;
    public string ClassroomName { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsActive { get; set; }
    public string? EndMethod { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public int? DurationMinutes { get; set; }
}
