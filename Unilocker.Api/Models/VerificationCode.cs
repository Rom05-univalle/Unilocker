namespace Unilocker.Api.Models;

/// <summary>
/// Modelo para códigos de verificación 2FA (almacenado en memoria)
/// </summary>
public class VerificationCode
{
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int FailedAttempts { get; set; }
}