using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Unilocker.Api.Data;
using Unilocker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar IHttpContextAccessor para auditoría
builder.Services.AddHttpContextAccessor();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<UnilockerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== CONFIGURACIÓN JWT =====
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Registrar servicio JWT
builder.Services.AddScoped<JwtService>();
// ===== FIN CONFIGURACIÓN JWT =====
// Registrar servicio Email
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<VerificationCodeService>();
// Registrar servicio de generación de contraseñas
builder.Services.AddScoped<PasswordGeneratorService>();

// Configurar CORS (para frontend web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Comentado porque usamos HTTP en desarrollo

// ===== ORDEN IMPORTANTE =====
app.UseCors("AllowAll");
app.UseAuthentication();  // <-- Antes de UseAuthorization
app.UseAuthorization();
// ===== FIN ORDEN =====

app.MapControllers();

// Endpoint de Health Check
app.MapGet("/api/health", async (UnilockerDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status = "healthy",
            connected = canConnect,
            timestamp = DateTime.UtcNow,
            database = "UnilockerDBV1"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "unhealthy",
            connected = false,
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

// Endpoint temporal para generar hash BCrypt
app.MapPost("/api/generate-hash", (string password) =>
{
    var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
    return Results.Ok(new { password, hash });
});

app.Run();