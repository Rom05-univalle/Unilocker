using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models;

public class LoginResponse
{
    public string? Token { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? RoleName { get; set; }
    public int? RoleId { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Propiedades para 2FA
    public bool RequiresVerification { get; set; }
    public string? MaskedEmail { get; set; }
}
