USE [UnilockerDBV1]
GO

-- Desactivar restricciones de foreign keys temporalmente
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Limpiar todas las tablas en orden inverso a las dependencias
DELETE FROM [dbo].[AuditLog];
DELETE FROM [dbo].[Report];
DELETE FROM [dbo].[Session];
DELETE FROM [dbo].[User];
DELETE FROM [dbo].[Computer];
DELETE FROM [dbo].[Classroom];
DELETE FROM [dbo].[Block];
DELETE FROM [dbo].[Branch];
DELETE FROM [dbo].[ProblemType];
DELETE FROM [dbo].[Role];

-- Reiniciar los contadores de identidad
DBCC CHECKIDENT ('[dbo].[AuditLog]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Report]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Session]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[User]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Computer]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Classroom]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Block]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Branch]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ProblemType]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Role]', RESEED, 0);

-- Reactivar restricciones de foreign keys
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

-- =============================================
-- INSERCI N DE DATOS
-- =============================================

-- 1. Roles
INSERT INTO [dbo].[Role] ([Name], [Description], [Status], [CreatedAt])
VALUES 
('Administrador', 'Acceso completo al sistema', 1, GETDATE()),
('Docente', 'Gesti n de sesiones y reportes', 1, GETDATE()),
('Estudiante', 'Uso de computadoras y reporte de problemas', 1, GETDATE());

-- 2. Sucursales (Branches)
INSERT INTO [dbo].[Branch] ([Name], [Address], [Code], [Status], [CreatedAt])
VALUES 
('Sucursal America', 'Av. America', 'AME', 1, GETDATE()),
('Sucursal Tiquipaya', 'Zona Tiquipaya', 'TIQ', 1, GETDATE()),
('Sucursal Ayacucho', 'Av. Ayacucho', 'AYA', 1, GETDATE());

-- 3. Bloques (Blocks)
INSERT INTO [dbo].[Block] ([Name], [Address], [BranchId], [Status], [CreatedAt])
VALUES 
-- Bloques Sucursal America (BranchId = 1)
('Bloque A', 'Edificio Principal', 1, 1, GETDATE()),
('Bloque B', 'Edificio Anexo', 1, 1, GETDATE()),
-- Bloques Sucursal Tiquipaya (BranchId = 2)
('Bloque T', 'Facultad de Informatica y Electronica', 2, 1, GETDATE()),
('Bloque S', 'Facultad de Tecnologia', 2, 1, GETDATE()),
-- Bloques Sucursal Ayacucho (BranchId = 3)
('Bloque F', 'Edificio Unico', 3, 1, GETDATE()),
('Bloque H', 'Edificio Posterior', 3, 1, GETDATE());

-- 4. Aulas (Classrooms)
INSERT INTO [dbo].[Classroom] ([Name], [Capacity], [BlockId], [Status], [CreatedAt])
VALUES 
-- Aulas Bloque A (BlockId = 1)
('A-101', 30, 1, 1, GETDATE()),
('A-102', 25, 1, 1, GETDATE()),
('A-103', 35, 1, 1, GETDATE()),
-- Aulas Bloque B (BlockId = 2)
('B-201', 28, 2, 1, GETDATE()),
('B-202', 30, 2, 1, GETDATE()),
-- Aulas Bloque T (BlockId = 3)
('T-100', 32, 3, 1, GETDATE()),
('T-8', 30, 3, 1, GETDATE()),
-- Aulas Bloque S (BlockId = 4)
('S-100', 25, 4, 1, GETDATE()),
('S-200', 28, 4, 1, GETDATE()),
-- Aulas Bloque F (BlockId = 5)
('F-13', 35, 5, 1, GETDATE()),
('F-103', 30, 5, 1, GETDATE()),
-- Aulas Bloque H (BlockId = 6)
('H-9', 25, 6, 1, GETDATE()),
('H-202', 25, 6, 1, GETDATE());

-- 5. Computadoras (Computers)
INSERT INTO [dbo].[Computer] ([Name], [SerialNumber], [Model], [ClassroomId], [Status], [CreatedAt])
VALUES 
-- Computadoras A-101 (ClassroomId = 1)
('A101-1', 'SN001A101', 'Dell OptiPlex 7090', 1, 1, GETDATE()),
('A101-2', 'SN002A101', 'Dell OptiPlex 7090', 1, 1, GETDATE()),
('A101-3', 'SN003A101', 'Dell OptiPlex 7090', 1, 1, GETDATE()),
('A101-4', 'SN004A101', 'HP ProDesk 400', 1, 1, GETDATE()),
('A101-5', 'SN005A101', 'HP ProDesk 400', 1, 1, GETDATE()),
-- Computadoras A-102 (ClassroomId = 2)
('A102-1', 'SN001A102', 'Lenovo ThinkCentre M90', 2, 1, GETDATE()),
('A102-2', 'SN002A102', 'Lenovo ThinkCentre M90', 2, 1, GETDATE()),
('A102-3', 'SN003A102', 'Dell OptiPlex 7090', 2, 1, GETDATE()),
('A102-4', 'SN004A102', 'Dell OptiPlex 7090', 2, 1, GETDATE()),
-- Computadoras A-103 (ClassroomId = 3)
('A103-1', 'SN001A103', 'HP ProDesk 600', 3, 1, GETDATE()),
('A103-2', 'SN002A103', 'HP ProDesk 600', 3, 1, GETDATE()),
('A103-3', 'SN003A103', 'HP ProDesk 600', 3, 1, GETDATE()),
('A103-4', 'SN004A103', 'Lenovo ThinkCentre M90', 3, 1, GETDATE()),
('A103-5', 'SN005A103', 'Lenovo ThinkCentre M90', 3, 1, GETDATE()),
-- Computadoras B-201 (ClassroomId = 4)
('B201-1', 'SN001B201', 'Dell OptiPlex 7090', 4, 1, GETDATE()),
('B201-2', 'SN002B201', 'Dell OptiPlex 7090', 4, 1, GETDATE()),
('B201-3', 'SN003B201', 'HP ProDesk 400', 4, 1, GETDATE()),
('B201-4', 'SN004B201', 'HP ProDesk 400', 4, 1, GETDATE()),
-- Computadoras B-202 (ClassroomId = 5)
('B202-1', 'SN001B202', 'Lenovo ThinkCentre M90', 5, 1, GETDATE()),
('B202-2', 'SN002B202', 'Lenovo ThinkCentre M90', 5, 1, GETDATE()),
('B202-3', 'SN003B202', 'Dell OptiPlex 7090', 5, 1, GETDATE()),
('B202-4', 'SN004B202', 'Dell OptiPlex 7090', 5, 1, GETDATE()),
-- Computadoras T-100 (ClassroomId = 6)
('T100-1', 'SN001T100', 'HP ProDesk 600', 6, 1, GETDATE()),
('T100-2', 'SN002T100', 'HP ProDesk 600', 6, 1, GETDATE()),
('T100-3', 'SN003T100', 'Lenovo ThinkCentre M90', 6, 1, GETDATE()),
('T100-4', 'SN004T100', 'Lenovo ThinkCentre M90', 6, 1, GETDATE()),
-- Computadoras T-8 (ClassroomId = 7)
('T8-1', 'SN001T8', 'Dell OptiPlex 7090', 7, 1, GETDATE()),
('T8-2', 'SN002T8', 'Dell OptiPlex 7090', 7, 1, GETDATE()),
('T8-3', 'SN003T8', 'HP ProDesk 400', 7, 1, GETDATE()),
('T8-4', 'SN004T8', 'HP ProDesk 400', 7, 1, GETDATE()),
-- Computadoras S-100 (ClassroomId = 8)
('S100-1', 'SN001S100', 'Lenovo ThinkCentre M90', 8, 1, GETDATE()),
('S100-2', 'SN002S100', 'Lenovo ThinkCentre M90', 8, 1, GETDATE()),
('S100-3', 'SN003S100', 'Dell OptiPlex 7090', 8, 1, GETDATE()),
-- Computadoras S-200 (ClassroomId = 9)
('S200-1', 'SN001S200', 'HP ProDesk 600', 9, 1, GETDATE()),
('S200-2', 'SN002S200', 'HP ProDesk 600', 9, 1, GETDATE()),
('S200-3', 'SN003S200', 'Dell OptiPlex 7090', 9, 1, GETDATE()),
-- Computadoras F-13 (ClassroomId = 10)
('F13-1', 'SN001F13', 'HP ProDesk 600', 10, 1, GETDATE()),
('F13-2', 'SN002F13', 'HP ProDesk 600', 10, 1, GETDATE()),
('F13-3', 'SN003F13', 'Lenovo ThinkCentre M90', 10, 1, GETDATE()),
('F13-4', 'SN004F13', 'Lenovo ThinkCentre M90', 10, 1, GETDATE()),
-- Computadoras F-103 (ClassroomId = 11)
('F103-1', 'SN001F103', 'Dell OptiPlex 7090', 11, 1, GETDATE()),
('F103-2', 'SN002F103', 'Dell OptiPlex 7090', 11, 1, GETDATE()),
('F103-3', 'SN003F103', 'HP ProDesk 400', 11, 1, GETDATE()),
-- Computadoras H-9 (ClassroomId = 12)
('H9-1', 'SN001H9', 'Lenovo ThinkCentre M90', 12, 1, GETDATE()),
('H9-2', 'SN002H9', 'Lenovo ThinkCentre M90', 12, 1, GETDATE()),
('H9-3', 'SN003H9', 'Dell OptiPlex 7090', 12, 1, GETDATE()),
-- Computadoras H-202 (ClassroomId = 13)
('H202-1', 'SN001H202', 'HP ProDesk 600', 13, 1, GETDATE()),
('H202-2', 'SN002H202', 'HP ProDesk 600', 13, 1, GETDATE()),
('H202-3', 'SN003H202', 'Dell OptiPlex 7090', 13, 1, GETDATE());

-- 6. Tipos de Problemas (ProblemTypes)
INSERT INTO [dbo].[ProblemType] ([Name], [Description], [Status], [CreatedAt])
VALUES 
('Hardware', 'Problemas con componentes fisicos', 1, GETDATE()),
('Software', 'Problemas con aplicaciones y sistema operativo', 1, GETDATE()),
('Red', 'Problemas de conectividad y red', 1, GETDATE()),
('Periferico', 'Problemas con mouse, teclado, monitor', 1, GETDATE()),
('Rendimiento', 'Computadora lenta o con bajo rendimiento', 1, GETDATE()),
('Otro', 'Otros problemas no especificados', 1, GETDATE());

-- 7. Usuarios (Users) - COMPLETAR CON TUS DATOS
-- NOTA: Debes reemplazar 'TU_PASSWORD_HASH_AQUI' con el hash real de la contrase a
INSERT INTO [dbo].[User] ([FirstName], [LastName], [SecondLastName], [Username], [Email], [PasswordHash], [Phone], [RoleId], [Status], [CreatedAt])
VALUES 
-- Usuario 1 - Administrador
('Rodrigo', 'Gutierrez', 'Herrera', 'radmin', 'ro4t5sld@gmail.com', '$2a$12$7eL4fuEygUVj141pp9fUi.aA71AmlotltUsKULe/PX66HRNwXW/B.', '68683588', 1, 1, GETDATE()),
-- Usuario 2 - Docente
('Rommel', 'Gutierrez', 'Herrera', 'ruser', 'ro4t5sld10@gmail.com', '$2a$12$7eL4fuEygUVj141pp9fUi.aA71AmlotltUsKULe/PX66HRNwXW/B.', '68683599', 2, 1, GETDATE()),
-- Usuario 3 - Estudiante
('Estudiante', 'Lopez', 'Garcia', 'estuser', 'email@ejemplo.com', '$2a$12$7eL4fuEygUVj141pp9fUi.aA71AmlotltUsKULe/PX66HRNwXW/B.', '77777777', 3, 1, GETDATE());

-- 8. Sesiones (Sessions) - Algunas sesiones de ejemplo
INSERT INTO [dbo].[Session] ([UserId], [ComputerId], [StartDateTime], [EndDateTime], [IsActive], [EndMethod], [LastHeartbeat], [CreatedAt])
VALUES 
-- Sesiones finalizadas
(3, 1, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -5, DATEADD(HOUR, 2, GETDATE())), 0, 'Normal', DATEADD(DAY, -5, DATEADD(HOUR, 2, GETDATE())), DATEADD(DAY, -5, GETDATE())),
(3, 2, DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -4, DATEADD(HOUR, 1, GETDATE())), 0, 'Normal', DATEADD(DAY, -4, DATEADD(HOUR, 1, GETDATE())), DATEADD(DAY, -4, GETDATE())),
(2, 15, DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -3, DATEADD(HOUR, 3, GETDATE())), 0, 'Normal', DATEADD(DAY, -3, DATEADD(HOUR, 3, GETDATE())), DATEADD(DAY, -3, GETDATE())),
(3, 5, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -2, DATEADD(HOUR, 2, GETDATE())), 0, 'Normal', DATEADD(DAY, -2, DATEADD(HOUR, 2, GETDATE())), DATEADD(DAY, -2, GETDATE())),
(3, 10, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, DATEADD(HOUR, 1, GETDATE())), 0, 'Forced', DATEADD(DAY, -1, DATEADD(HOUR, 1, GETDATE())), DATEADD(DAY, -1, GETDATE())),
-- Sesi n activa actual
(3, 3, DATEADD(HOUR, -1, GETDATE()), NULL, 1, NULL, GETDATE(), DATEADD(HOUR, -1, GETDATE()));

-- 9. Reportes (Reports) - Algunos reportes de ejemplo
INSERT INTO [dbo].[Report] ([SessionId], [ProblemTypeId], [Description], [ReportDate], [ReportStatus], [ResolutionDate], [CreatedAt])
VALUES 
(1, 4, 'El mouse no responde correctamente, se traba constantemente', DATEADD(DAY, -5, GETDATE()), 'Resolved', DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -5, GETDATE())),
(2, 2, 'El navegador Chrome se cierra inesperadamente', DATEADD(DAY, -4, GETDATE()), 'Resolved', DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -4, GETDATE())),
(3, 3, 'No hay conexion a Internet, no puedo acceder a los recursos en linea', DATEADD(DAY, -3, GETDATE()), 'InReview', NULL, DATEADD(DAY, -3, GETDATE())),
(4, 5, 'La computadora esta muy lenta, tarda mucho en abrir programas', DATEADD(DAY, -2, GETDATE()), 'Pending', NULL, DATEADD(DAY, -2, GETDATE())),
(5, 1, 'La computadora se apaga sola despues de unos minutos de uso', DATEADD(DAY, -1, GETDATE()), 'Pending', NULL, DATEADD(DAY, -1, GETDATE()));

GO
