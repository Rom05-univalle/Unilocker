#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

// Test del hash existente
string password = "123";
string existingHash = "$2a$12$7eL4fuEygUVj141pp9fUi.aA71AmlotltUsKULe/PX66HRNwXW/B.";

Console.WriteLine($"Password a verificar: {password}");
Console.WriteLine($"Hash existente: {existingHash}");
Console.WriteLine($"¿El hash es válido? {BCrypt.Verify(password, existingHash)}");
Console.WriteLine();

// Generar nuevo hash
string newHash = BCrypt.HashPassword(password, 12);
Console.WriteLine($"Nuevo hash generado: {newHash}");
Console.WriteLine($"¿El nuevo hash es válido? {BCrypt.Verify(password, newHash)}");
