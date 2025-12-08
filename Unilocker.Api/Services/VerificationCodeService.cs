using System.Collections.Concurrent;
using Unilocker.Api.Models;

namespace Unilocker.Api.Services;

/// <summary>
/// Servicio para gestionar códigos de verificación 2FA en memoria
/// </summary>
public class VerificationCodeService
{
    // Diccionario thread-safe para almacenar códigos
    private static readonly ConcurrentDictionary<int, VerificationCode> _codes = new();
    private readonly ILogger<VerificationCodeService> _logger;

    public VerificationCodeService(ILogger<VerificationCodeService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Genera un código de 6 dígitos aleatorio
    /// </summary>
    public string GenerateCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Guarda un código de verificación para un usuario
    /// </summary>
    public void SaveCode(int userId, string code)
    {
        var verificationCode = new VerificationCode
        {
            UserId = userId,
            Code = code,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            IsUsed = false,
            FailedAttempts = 0
        };

        _codes[userId] = verificationCode;
        _logger.LogInformation("✅ Código guardado para UserId: {UserId}", userId);
    }

    /// <summary>
    /// Valida un código de verificación
    /// </summary>
    public (bool isValid, string message) ValidateCode(int userId, string code)
    {
        _logger.LogInformation("🔍 Validando código para UserId: {UserId}", userId);

        // Verificar si existe código para el usuario
        if (!_codes.TryGetValue(userId, out var storedCode))
        {
            _logger.LogWarning("❌ No existe código para UserId: {UserId}", userId);
            return (false, "No se encontró código de verificación");
        }

        // Verificar si ya fue usado
        if (storedCode.IsUsed)
        {
            _logger.LogWarning("❌ Código ya usado para UserId: {UserId}", userId);
            return (false, "El código ya fue utilizado");
        }

        // Verificar si expiró
        if (DateTime.Now > storedCode.ExpiresAt)
        {
            _logger.LogWarning("❌ Código expirado para UserId: {UserId}", userId);
            _codes.TryRemove(userId, out _);
            return (false, "El código ha expirado");
        }

        // Verificar intentos fallidos (máximo 3)
        if (storedCode.FailedAttempts >= 3)
        {
            _logger.LogWarning("❌ Máximo de intentos alcanzado para UserId: {UserId}", userId);
            _codes.TryRemove(userId, out _);
            return (false, "Máximo de intentos alcanzado. Solicita un nuevo código");
        }

        // Verificar el código
        if (storedCode.Code != code)
        {
            storedCode.FailedAttempts++;
            _logger.LogWarning("❌ Código incorrecto para UserId: {UserId}. Intento {Attempt}/3",
                userId, storedCode.FailedAttempts);
            return (false, $"Código incorrecto. Intentos restantes: {3 - storedCode.FailedAttempts}");
        }

        // Código válido - marcarlo como usado
        storedCode.IsUsed = true;
        _logger.LogInformation("✅ Código válido para UserId: {UserId}", userId);

        // Eliminar después de 5 segundos (para limpiar memoria)
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            _codes.TryRemove(userId, out _);
        });

        return (true, "Código válido");
    }

    /// <summary>
    /// Elimina el código de un usuario (útil para limpiar)
    /// </summary>
    public void RemoveCode(int userId)
    {
        _codes.TryRemove(userId, out _);
        _logger.LogInformation("🗑️ Código eliminado para UserId: {UserId}", userId);
    }

    /// <summary>
    /// Verifica si existe un código activo para un usuario
    /// </summary>
    public bool HasActiveCode(int userId)
    {
        if (_codes.TryGetValue(userId, out var code))
        {
            return !code.IsUsed && DateTime.Now <= code.ExpiresAt;
        }
        return false;
    }
}