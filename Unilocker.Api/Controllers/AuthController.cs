using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;
using Unilocker.Api.Services;
using Unilocker.Api.DTOs; // aquí están LoginRequest, VerifyCodeRequest, ResendCodeRequest
using LoginRequestDto = Unilocker.Api.DTOs.LoginRequest;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UnilockerDbContext context;
    private readonly IPasswordHasher<object> passwordHasher;
    private readonly JwtService jwtService;
    private readonly IEmailService emailService;

    public AuthController(
    UnilockerDbContext context,
    IPasswordHasher<object> passwordHasher,
    JwtService jwtService,
    IEmailService emailService)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
        this.jwtService = jwtService;
        this.emailService = emailService;
    }


    // POST: /api/auth/login
    // Paso 1: validar user/pass, generar código y enviarlo por correo
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            Console.WriteLine("Login: username o password vacíos");
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        Console.WriteLine($"Login: intentando usuario '{request.Username}'");

        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username &&
                u.Status == true &&
                (u.IsBlocked == null || u.IsBlocked == false));

        if (user == null)
        {
            Console.WriteLine("Login: usuario no encontrado / inactivo / bloqueado");
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        Console.WriteLine($"Login: resultado VerifyHashedPassword = {result}");

        if (result == PasswordVerificationResult.Failed)
        {
            Console.WriteLine("Login: hash FAILED");
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        Console.WriteLine("Login: credenciales OK, generando código 2FA");
        // ----- 2FA por correo -----
        var rnd = new Random();
        var code = rnd.Next(100000, 999999).ToString(); // 6 dígitos
        Console.WriteLine($"Login: código 2FA generado para user {user.Id}: {code}");
        var twoFactor = new TwoFactorCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Used = false
        };

        context.TwoFactorCodes.Add(twoFactor);
        await context.SaveChangesAsync();

        // Enviar código al email del usuario
        var subject = "Código de verificación UniLocker";
        var body = $"Tu código de verificación es: {code}";
        await emailService.SendAsync(user.Email, subject, body);

        // El frontend (auth.js/login.js) espera esto
        return Ok(new
        {
            requiresVerification = true,
            userId = user.Id
        });
    }

    // POST: /api/auth/verify-code
    // Paso 2: validar código y devolver token + datos de usuario
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return Unauthorized(new { message = "Usuario inválido" });

        var codeEntry = await context.TwoFactorCodes
            .Where(c => c.UserId == request.UserId && c.Code == request.Code && !c.Used)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();

        if (codeEntry == null || codeEntry.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Código inválido o expirado" });
        }

        codeEntry.Used = true;
        await context.SaveChangesAsync();

        var token = jwtService.GenerateToken(user);
        var expiresAt = jwtService.GetTokenExpirationTime();

        return Ok(new
        {
            token,
            expiresAt,
            user = new
            {
                user.Id,
                user.Username,
                user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role != null ? user.Role.Name : null
            }
        });
    }

    // POST: /api/auth/resend-code
    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendCode([FromBody] ResendCodeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
            return Unauthorized(new { message = "Usuario inválido" });

        // Invalidar códigos anteriores vigentes
        var existing = await context.TwoFactorCodes
            .Where(c => c.UserId == request.UserId && !c.Used && c.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var c in existing)
            c.Used = true;

        var rnd = new Random();
        var code = rnd.Next(100000, 999999).ToString();

        var twoFactor = new TwoFactorCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Used = false
        };

        context.TwoFactorCodes.Add(twoFactor);
        await context.SaveChangesAsync();

        var subject = "Nuevo código de verificación UniLocker";
        var body = $"Tu nuevo código de verificación es: {code}";
        await emailService.SendAsync(user.Email, subject, body);

        return Ok();
    }
}