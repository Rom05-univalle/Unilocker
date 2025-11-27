namespace Unilocker.Api.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public int RoleId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
