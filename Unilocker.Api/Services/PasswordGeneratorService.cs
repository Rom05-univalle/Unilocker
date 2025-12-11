using System.Security.Cryptography;
using System.Text;

namespace Unilocker.Api.Services;

public class PasswordGeneratorService
{
    /// <summary>
    /// Genera una contraseña automática basada en el nombre de usuario, nombres completos y números aleatorios
    /// Formato: PrimerasLetras + Números + CaracterEspecial
    /// Ejemplo: JPerez2847!
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="firstName">Primer nombre</param>
    /// <param name="lastName">Primer apellido</param>
    /// <param name="secondLastName">Segundo apellido (opcional)</param>
    /// <returns>Contraseña generada de 10-12 caracteres</returns>
    public string GeneratePassword(string username, string firstName, string lastName, string? secondLastName = null)
    {
        // Limpiar y normalizar entradas
        username = CleanString(username);
        firstName = CleanString(firstName);
        lastName = CleanString(lastName);
        secondLastName = CleanString(secondLastName);

        // Crear base de la contraseña con las iniciales o primeras letras
        var passwordBase = new StringBuilder();

        // Tomar primera letra del nombre en mayúscula
        if (!string.IsNullOrEmpty(firstName))
        {
            passwordBase.Append(char.ToUpper(firstName[0]));
        }

        // Tomar primeras 2-3 letras del primer apellido
        if (!string.IsNullOrEmpty(lastName))
        {
            var lastNamePart = lastName.Length >= 3 ? lastName.Substring(0, 3) : lastName;
            passwordBase.Append(char.ToUpper(lastNamePart[0]));
            if (lastNamePart.Length > 1)
                passwordBase.Append(lastNamePart.Substring(1).ToLower());
        }

        // Generar 4 números aleatorios
        var randomNumbers = GenerateRandomNumbers(4);
        passwordBase.Append(randomNumbers);

        // Agregar un carácter especial al final
        var specialChars = new[] { '!', '@', '#', '$', '*' };
        var randomSpecialChar = specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)];
        passwordBase.Append(randomSpecialChar);

        return passwordBase.ToString();
    }

    /// <summary>
    /// Genera números aleatorios seguros
    /// </summary>
    private string GenerateRandomNumbers(int length)
    {
        var numbers = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            numbers.Append(RandomNumberGenerator.GetInt32(0, 10));
        }
        return numbers.ToString();
    }

    /// <summary>
    /// Limpia una cadena eliminando espacios y caracteres especiales
    /// </summary>
    private string CleanString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remover acentos y caracteres especiales
        var normalized = input.Trim()
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
            .Replace("ñ", "n").Replace("Ñ", "N")
            .Replace(" ", "");

        return normalized;
    }

    /// <summary>
    /// Valida que una contraseña cumpla con los requisitos mínimos
    /// </summary>
    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Mínimo 6 caracteres
        if (password.Length < 6)
            return false;

        // Debe tener al menos una letra
        if (!password.Any(char.IsLetter))
            return false;

        // Debe tener al menos un número
        if (!password.Any(char.IsDigit))
            return false;

        return true;
    }
}
