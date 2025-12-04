namespace Unilocker.Api.DTOs;

public class LoginResponse
{
    public string? Token { get; set; } // Ahora es nullable
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? RoleName { get; set; }
    public int? RoleId { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Nuevas propiedades para 2FA
    public bool RequiresVerification { get; set; }
    public string? MaskedEmail { get; set; }
}