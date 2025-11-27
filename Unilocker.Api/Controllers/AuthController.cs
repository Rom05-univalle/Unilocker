using Microsoft.AspNetCore.Mvc;
using Unilocker.Api.Models;
using Unilocker.Api.Services;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {

        if (request.Username != "admin" || request.Password != "admin")
        {
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        var user = new User
        {
            Id = 1,
            Username = "admin",
            FirstName = "Admin",
            LastName = "User",
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" }
        };

        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetTokenExpirationTime();

        return Ok(new { token, expiresAt });
    }
}
