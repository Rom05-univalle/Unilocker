using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models;

public class StartSessionRequest
{
    public int UserId { get; set; }
    public int ComputerId { get; set; }
}
