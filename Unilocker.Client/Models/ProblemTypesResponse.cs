using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Unilocker.Client.Models;

public class ProblemTypesResponse
{
    public List<ProblemType> ProblemTypes { get; set; } = new();
    public int Count { get; set; }
}
