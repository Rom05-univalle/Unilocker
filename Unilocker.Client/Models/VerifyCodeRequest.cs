using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models;

public class VerifyCodeRequest
{
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
}