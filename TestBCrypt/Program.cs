using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        // Test del hash existente
        string password = "123";
        string existingHash = "$2a$12$7eL4fuEygUVj141pp9fUi.aA71AmlotltUsKULe/PX66HRNwXW/B.";

        Console.WriteLine($"Password a verificar: {password}");
        Console.WriteLine($"Hash existente: {existingHash}");
        
        try
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, existingHash);
            Console.WriteLine($"¿El hash es válido? {isValid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verificando hash: {ex.Message}");
        }
        
        Console.WriteLine();

        // Generar nuevo hash
        try
        {
            string newHash = BCrypt.Net.BCrypt.HashPassword(password, 12);
            Console.WriteLine($"Nuevo hash generado: {newHash}");
            Console.WriteLine($"¿El nuevo hash es válido? {BCrypt.Net.BCrypt.Verify(password, newHash)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando hash: {ex.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("=== Hash para contraseña '123456' ===");
        string newPassword = "123456";
        string hashFor123456 = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
        Console.WriteLine($"Contraseña: {newPassword}");
        Console.WriteLine($"Hash: {hashFor123456}");
        Console.WriteLine($"Verificación: {BCrypt.Net.BCrypt.Verify(newPassword, hashFor123456)}");
    }
}
