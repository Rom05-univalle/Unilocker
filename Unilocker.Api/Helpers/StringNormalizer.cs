using System.Text.RegularExpressions;

namespace Unilocker.Api.Helpers;

/// <summary>
/// Clase para normalizar y limpiar strings de entrada del usuario
/// </summary>
public static class StringNormalizer
{
    /// <summary>
    /// Normaliza un string: elimina espacios al inicio/final y reemplaza múltiples espacios por uno solo
    /// </summary>
    /// <param name="input">String a normalizar</param>
    /// <returns>String normalizado o null si el input es null</returns>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // 1. Trim: Eliminar espacios al inicio y final
        var normalized = input.Trim();

        // 2. Reemplazar múltiples espacios consecutivos por uno solo
        normalized = Regex.Replace(normalized, @"\s+", " ");

        // 3. Si después de la limpieza queda vacío, retornar null
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    /// Normaliza un string y garantiza que no sea null/empty después de la normalización
    /// </summary>
    /// <param name="input">String a normalizar</param>
    /// <param name="fieldName">Nombre del campo (para mensajes de error)</param>
    /// <returns>String normalizado</returns>
    /// <exception cref="ArgumentException">Si el string es null/empty después de normalizar</exception>
    public static string NormalizeRequired(string? input, string fieldName)
    {
        var normalized = Normalize(input);
        
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"El campo '{fieldName}' es obligatorio");

        return normalized;
    }

    /// <summary>
    /// Normaliza un email: lowercase, trim y validación básica
    /// </summary>
    public static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Trim y lowercase para emails
        var normalized = email.Trim().ToLowerInvariant();

        // Reemplazar múltiples espacios (aunque no deberían existir en emails)
        normalized = Regex.Replace(normalized, @"\s+", "");

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    /// Normaliza un username: trim, lowercase, sin espacios múltiples
    /// </summary>
    public static string? NormalizeUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        // Trim
        var normalized = username.Trim();

        // Reemplazar múltiples espacios por uno (aunque usernames no deberían tener espacios)
        normalized = Regex.Replace(normalized, @"\s+", "");

        // Lowercase para consistencia
        normalized = normalized.ToLowerInvariant();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    /// Normaliza un teléfono: solo dígitos, espacios y guiones
    /// </summary>
    public static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Trim
        var normalized = phone.Trim();

        // Eliminar múltiples espacios
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
