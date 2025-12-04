
namespace Unilocker.Api.DTOs;

public class VerifyCodeRequest
{
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
}
