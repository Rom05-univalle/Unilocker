
namespace Unilocker.Api.Models;

public class TwoFactorCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }

    public User User { get; set; } = null!;
}
