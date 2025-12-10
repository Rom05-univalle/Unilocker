-- Verificar usuario radmin
USE UnilockerDBV1;
GO

-- Ver datos del usuario
SELECT 
    u.Id,
    u.Username,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Status,
    u.IsBlocked,
    u.RoleId,
    u.PasswordHash,
    r.Name as RoleName
FROM [User] u
LEFT JOIN Role r ON u.RoleId = r.Id
WHERE u.Username = 'radmin';

-- Ver rol Administrador
SELECT Id, Name, Description, Status 
FROM Role 
WHERE Id = 1;

-- Generar nuevo hash para contraseña "123"
-- Para usar en código C#: BCrypt.Net.BCrypt.HashPassword("123", 12)
