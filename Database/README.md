# Base de Datos - Sistema Unilocker

Esta carpeta contiene los scripts SQL necesarios para crear y configurar la base de datos del sistema Unilocker.

## Archivos

### 01_CREATE_DATABASE.sql
Script de creación de la base de datos y todas las tablas necesarias:
- Roles
- Users (Usuarios)
- Branches (Sedes)
- Blocks (Bloques)
- Classrooms (Aulas/Laboratorios)
- Computers (Computadoras)
- Sessions (Sesiones)
- ProblemTypes (Tipos de Problemas)
- Reports (Reportes)
- AuditLogs (Auditoría)

### 02_INSERT_DATA.sql
Script de inserción de datos de ejemplo para pruebas:
- 3 Roles (Administrador, Usuario, Supervisor)
- 4 Usuarios de ejemplo
- 3 Sedes universitarias
- 6 Bloques
- 10 Laboratorios
- 15 Computadoras de ejemplo
- 7 Tipos de problemas
- Sesiones de ejemplo (activas e históricas)
- Reportes de ejemplo
- Logs de auditoría

## Instrucciones de Uso

### 1. Crear la base de datos
```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
sqlcmd -S localhost -i 01_CREATE_DATABASE.sql
```

### 2. Insertar datos de ejemplo
```sql
sqlcmd -S localhost -i 02_INSERT_DATA.sql
```

### 3. Verificar instalación
```sql
USE UnilockerDB;
GO

SELECT 
    'Roles' AS Tabla, COUNT(*) AS Registros FROM Roles UNION ALL
    SELECT 'Users', COUNT(*) FROM Users UNION ALL
    SELECT 'Computers', COUNT(*) FROM Computers UNION ALL
    SELECT 'Sessions', COUNT(*) FROM Sessions;
```

## Usuarios de Prueba

| Usuario | Contraseña | Rol | Email |
|---------|-----------|-----|-------|
| radmin | admin123 | Administrador | admin@univalle.edu |
| usuario1 | password123 | Usuario | usuario1@univalle.edu |
| usuario2 | password123 | Usuario | usuario2@univalle.edu |
| supervisor1 | password123 | Supervisor | supervisor@univalle.edu |

**Nota:** Las contraseñas están hasheadas con BCrypt. Para generar nuevos hashes, usa el script `generate-hash.ps1` en la raíz del proyecto.

## Diagrama de Relaciones

```
Roles (1) ──→ (N) Users
Branches (1) ──→ (N) Blocks
Blocks (1) ──→ (N) Classrooms
Classrooms (1) ──→ (N) Computers

Users (1) ──→ (N) Sessions
Computers (1) ──→ (N) Sessions

Users (1) ──→ (N) Reports
Computers (1) ──→ (N) Reports
ProblemTypes (1) ──→ (N) Reports

Users (1) ──→ (N) AuditLogs
```

## Requisitos

- SQL Server 2019 o superior
- SQL Server Management Studio (SSMS) o Azure Data Studio
- Permisos de administrador para crear bases de datos

## Configuración en la API

Actualizar la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UnilockerDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```
