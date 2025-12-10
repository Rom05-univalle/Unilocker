-- =============================================
-- Script de Inserción de Datos de Ejemplo
-- Sistema Unilocker - Control de Acceso a Laboratorios
-- =============================================

USE UnilockerDB;
GO

-- =============================================
-- 1. ROLES
-- =============================================
INSERT INTO Roles (Name, Description) VALUES
('Administrador', 'Acceso completo al sistema'),
('Usuario', 'Usuario estándar del sistema'),
('Supervisor', 'Supervisor de laboratorios');
GO

-- =============================================
-- 2. USUARIOS
-- =============================================
-- Contraseña para admin: admin123 (BCrypt hash)
-- Contraseña para usuarios: password123
INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, TwoFactorEnabled) VALUES
('radmin', '$2a$11$xVZV7J3qXJ3qXJ3qXJ3qXeFakeHashForAdminUser123456789', 'admin@univalle.edu', 'Administrador Sistema', 1, 0),
('usuario1', '$2a$11$xVZV7J3qXJ3qXJ3qXJ3qXeFakeHashForUser1234567890', 'usuario1@univalle.edu', 'Juan Pérez García', 2, 0),
('usuario2', '$2a$11$xVZV7J3qXJ3qXJ3qXJ3qXeFakeHashForUser2234567890', 'usuario2@univalle.edu', 'María López Torres', 2, 0),
('supervisor1', '$2a$11$xVZV7J3qXJ3qXJ3qXJ3qXeFakeHashForSupervisor123', 'supervisor@univalle.edu', 'Carlos Méndez Silva', 3, 0);
GO

-- =============================================
-- 3. INFRAESTRUCTURA
-- =============================================

-- Sedes
INSERT INTO Branches (Name, Address) VALUES
('Sede Central', 'Av. Universidad #123'),
('Sede Norte', 'Calle Norte #456'),
('Sede Sur', 'Boulevard Sur #789');
GO

-- Bloques
INSERT INTO Blocks (Name, BranchId, Description) VALUES
-- Sede Central
('Bloque A', 1, 'Edificio de Ingeniería'),
('Bloque B', 1, 'Edificio de Ciencias'),
('Bloque C', 1, 'Edificio de Arquitectura'),
-- Sede Norte
('Bloque N1', 2, 'Edificio Principal Norte'),
('Bloque N2', 2, 'Edificio de Laboratorios'),
-- Sede Sur
('Bloque S1', 3, 'Edificio de Tecnología');
GO

-- Aulas/Laboratorios
INSERT INTO Classrooms (Name, BlockId, Capacity, Description) VALUES
-- Bloque A (Sede Central)
('LAB-A-101', 1, 30, 'Laboratorio de Programación 1'),
('LAB-A-102', 1, 25, 'Laboratorio de Redes'),
('LAB-A-201', 1, 35, 'Laboratorio de Base de Datos'),
-- Bloque B (Sede Central)
('LAB-B-101', 2, 28, 'Laboratorio de Física'),
('LAB-B-102', 2, 30, 'Laboratorio de Química'),
-- Bloque C (Sede Central)
('LAB-C-101', 3, 20, 'Laboratorio de Diseño CAD'),
-- Bloque N1 (Sede Norte)
('LAB-N1-101', 4, 32, 'Laboratorio Multimedia'),
('LAB-N1-102', 4, 30, 'Laboratorio de Simulación'),
-- Bloque N2 (Sede Norte)
('LAB-N2-101', 5, 25, 'Laboratorio de IA'),
-- Bloque S1 (Sede Sur)
('LAB-S1-101', 6, 28, 'Laboratorio de Desarrollo Web');
GO

-- =============================================
-- 4. COMPUTADORAS
-- =============================================
INSERT INTO Computers (Name, Uuid, SerialNumber, Model, ClassroomId) VALUES
-- LAB-A-101 (30 computadoras)
('PC-A101-01', NEWID(), 'SN-A101-001', 'Dell OptiPlex 7080', 1),
('PC-A101-02', NEWID(), 'SN-A101-002', 'Dell OptiPlex 7080', 1),
('PC-A101-03', NEWID(), 'SN-A101-003', 'Dell OptiPlex 7080', 1),
('PC-A101-04', NEWID(), 'SN-A101-004', 'HP EliteDesk 800', 1),
('PC-A101-05', NEWID(), 'SN-A101-005', 'HP EliteDesk 800', 1),
-- LAB-A-102 (25 computadoras)
('PC-A102-01', NEWID(), 'SN-A102-001', 'Lenovo ThinkCentre M720', 2),
('PC-A102-02', NEWID(), 'SN-A102-002', 'Lenovo ThinkCentre M720', 2),
('PC-A102-03', NEWID(), 'SN-A102-003', 'Dell OptiPlex 5080', 2),
-- LAB-A-201 (35 computadoras)
('PC-A201-01', NEWID(), 'SN-A201-001', 'HP ProDesk 600', 3),
('PC-A201-02', NEWID(), 'SN-A201-002', 'HP ProDesk 600', 3),
('PC-A201-03', NEWID(), 'SN-A201-003', 'Dell Precision 3640', 3),
-- LAB-B-101
('PC-B101-01', NEWID(), 'SN-B101-001', 'Lenovo M920', 4),
('PC-B101-02', NEWID(), 'SN-B101-002', 'Lenovo M920', 4),
-- LAB-N1-101
('PC-N101-01', NEWID(), 'SN-N101-001', 'Dell OptiPlex 7090', 7),
('PC-N101-02', NEWID(), 'SN-N101-002', 'Dell OptiPlex 7090', 7);
GO

-- =============================================
-- 5. TIPOS DE PROBLEMAS
-- =============================================
INSERT INTO ProblemTypes (Name, Description) VALUES
('Hardware', 'Problemas de hardware (teclado, mouse, monitor)'),
('Software', 'Problemas de software o aplicaciones'),
('Red', 'Problemas de conexión a Internet'),
('Sistema Operativo', 'Problemas con Windows o sistema'),
('Impresora', 'Problemas con impresoras'),
('Periféricos', 'Problemas con dispositivos periféricos'),
('Otro', 'Otros problemas no clasificados');
GO

-- =============================================
-- 6. SESIONES DE EJEMPLO (Historial)
-- =============================================
INSERT INTO Sessions (UserId, ComputerId, StartDateTime, EndDateTime, DurationMinutes, IsActive) VALUES
-- Sesiones cerradas (historial)
(2, 1, DATEADD(day, -5, GETDATE()), DATEADD(day, -5, DATEADD(hour, 2, GETDATE())), 120, 0),
(3, 2, DATEADD(day, -5, GETDATE()), DATEADD(day, -5, DATEADD(hour, 1, GETDATE())), 60, 0),
(2, 3, DATEADD(day, -4, GETDATE()), DATEADD(day, -4, DATEADD(hour, 3, GETDATE())), 180, 0),
(3, 1, DATEADD(day, -3, GETDATE()), DATEADD(day, -3, DATEADD(hour, 2, GETDATE())), 120, 0),
(2, 4, DATEADD(day, -2, GETDATE()), DATEADD(day, -2, DATEADD(hour, 1, GETDATE())), 60, 0),
-- Sesiones activas (actuales)
(2, 5, DATEADD(hour, -1, GETDATE()), NULL, NULL, 1),
(3, 6, DATEADD(minute, -30, GETDATE()), NULL, NULL, 1);
GO

-- =============================================
-- 7. REPORTES DE EJEMPLO
-- =============================================
INSERT INTO Reports (ComputerId, UserId, ProblemTypeId, Description, Status, CreatedAt) VALUES
(1, 2, 1, 'El teclado no funciona correctamente, algunas teclas no responden', 'Pendiente', DATEADD(day, -2, GETDATE())),
(2, 3, 3, 'No hay conexión a Internet en esta computadora', 'En Proceso', DATEADD(day, -1, GETDATE())),
(3, 2, 2, 'Microsoft Office no abre correctamente', 'Resuelto', DATEADD(day, -3, GETDATE())),
(4, 3, 4, 'El sistema operativo se reinicia constantemente', 'Pendiente', GETDATE());
GO

-- Actualizar fecha de resolución para reportes resueltos
UPDATE Reports SET ResolvedAt = DATEADD(hour, 4, CreatedAt) WHERE Status = 'Resuelto';
GO

-- =============================================
-- 8. LOGS DE AUDITORÍA DE EJEMPLO
-- =============================================
INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, Details, IpAddress, Timestamp) VALUES
(1, 'LOGIN', 'User', 1, 'Inicio de sesión exitoso', '192.168.0.100', DATEADD(day, -1, GETDATE())),
(1, 'CREATE', 'Computer', 1, 'Registro de nueva computadora PC-A101-01', '192.168.0.100', DATEADD(day, -1, GETDATE())),
(2, 'LOGIN', 'User', 2, 'Inicio de sesión exitoso', '192.168.0.101', DATEADD(hour, -2, GETDATE())),
(2, 'START_SESSION', 'Session', 6, 'Inicio de sesión en PC-A101-05', '192.168.0.105', DATEADD(hour, -1, GETDATE())),
(3, 'CREATE_REPORT', 'Report', 2, 'Creación de reporte de problema de red', '192.168.0.102', DATEADD(day, -1, GETDATE()));
GO

-- =============================================
-- Verificación de datos insertados
-- =============================================
PRINT 'Datos de ejemplo insertados correctamente';
PRINT '==========================================';
SELECT 'Roles' AS Tabla, COUNT(*) AS Registros FROM Roles UNION ALL
SELECT 'Users', COUNT(*) FROM Users UNION ALL
SELECT 'Branches', COUNT(*) FROM Branches UNION ALL
SELECT 'Blocks', COUNT(*) FROM Blocks UNION ALL
SELECT 'Classrooms', COUNT(*) FROM Classrooms UNION ALL
SELECT 'Computers', COUNT(*) FROM Computers UNION ALL
SELECT 'ProblemTypes', COUNT(*) FROM ProblemTypes UNION ALL
SELECT 'Sessions', COUNT(*) FROM Sessions UNION ALL
SELECT 'Reports', COUNT(*) FROM Reports UNION ALL
SELECT 'AuditLogs', COUNT(*) FROM AuditLogs;
GO
