-- Actualizar contraseña de cmamani
-- Hash BCrypt para "Admin123!" con 12 rounds
UPDATE [User] 
SET PasswordHash = '$2a$12$WrZuwfKCEZwdzYGTvhbklu2KiyUoZBqWvufOrxq8NKGv4lh4Rsi3W',
    IsBlocked = 0,
    FailedLoginAttempts = 0
WHERE Username = 'cmamani';

-- Verificar actualización
SELECT Id, Username, LEFT(PasswordHash, 20) AS HashPrefix, LEN(PasswordHash) AS HashLength, IsBlocked, FailedLoginAttempts
FROM [User] 
WHERE Username = 'cmamani';
