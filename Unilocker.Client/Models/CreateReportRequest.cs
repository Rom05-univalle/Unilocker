using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models;

public class CreateReportRequest
{
    public int SessionId { get; set; }
    public int ProblemTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
}
