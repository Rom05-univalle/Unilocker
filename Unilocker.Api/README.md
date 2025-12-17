# Unilocker API

API REST Backend del sistema Unilocker para control de acceso a laboratorios.

## Descripción

Backend centralizado que gestiona toda la lógica de negocio del sistema Unilocker. Proporciona endpoints RESTful para autenticación, gestión de sesiones, administración de infraestructura y reportes.

## Características

- Autenticación JWT con tokens seguros
- Autorización basada en roles
- 2FA opcional vía correo electrónico
- CRUD completo para todas las entidades
- Control de sesiones activas con heartbeat
- Sistema de reportes de problemas
- Auditoría automática de operaciones
- Eliminación lógica de registros
- Validación de datos con DataAnnotations
- Manejo de errores centralizado

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- BCrypt.Net para hashing de contraseñas
- System.Text.Json para serialización

## Estructura del Proyecto

```
Unilocker.Api/
├── Controllers/          # Controladores de la API
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── SessionsController.cs
│   ├── ComputersController.cs
│   ├── ReportsController.cs
│   ├── BranchesController.cs
│   ├── BlocksController.cs
│   ├── ClassroomsController.cs
│   ├── DashboardController.cs
│   └── AuditController.cs
├── Data/
│   └── UnilockerDbContext.cs
├── DTOs/                 # Data Transfer Objects
│   ├── LoginRequest.cs
│   ├── SessionResponse.cs
│   └── ...
├── Extensions/           # Métodos de extensión
│   └── HttpContextExtensions.cs
├── Helpers/             # Clases auxiliares
│   └── AuditHelper.cs
├── Models/              # Entidades de base de datos
│   ├── User.cs
│   ├── Computer.cs
│   ├── Session.cs
│   └── ...
├── Services/            # Servicios de la aplicación
│   ├── EmailService.cs
│   └── TwoFactorService.cs
├── Program.cs           # Punto de entrada
└── appsettings.json     # Configuración
```

## Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/request-2fa` - Solicitar código 2FA
- `POST /api/auth/verify-2fa` - Verificar código 2FA

### Usuarios
- `GET /api/users` - Listar usuarios
- `GET /api/users/{id}` - Obtener usuario
- `POST /api/users` - Crear usuario
- `PUT /api/users/{id}` - Actualizar usuario
- `DELETE /api/users/{id}` - Eliminar usuario (lógico)

### Sesiones
- `POST /api/sessions/start` - Iniciar sesión
- `PUT /api/sessions/{id}/end` - Finalizar sesión
- `PUT /api/sessions/{id}/heartbeat` - Actualizar heartbeat
- `GET /api/sessions` - Listar sesiones
- `GET /api/sessions/{id}` - Obtener sesión
- `GET /api/sessions/active` - Sesiones activas

### Computadoras
- `GET /api/computers` - Listar computadoras
- `GET /api/computers/{id}` - Obtener computadora
- `POST /api/computers/register` - Registrar computadora (público)
- `PUT /api/computers/{id}` - Actualizar computadora
- `PUT /api/computers/{id}/status` - Cambiar estado
- `DELETE /api/computers/{id}` - Desregistrar (lógico)

### Reportes
- `GET /api/reports` - Listar reportes
- `GET /api/reports/{id}` - Obtener reporte
- `POST /api/reports` - Crear reporte
- `PUT /api/reports/{id}` - Actualizar reporte
- `PUT /api/reports/{id}/status` - Cambiar estado
- `DELETE /api/reports/{id}` - Eliminar reporte (lógico)

### Infraestructura
- `GET /api/branches` - Listar sucursales
- `GET /api/blocks` - Listar bloques
- `GET /api/classrooms` - Listar aulas
- POST/PUT/DELETE para cada entidad

### Dashboard
- `GET /api/dashboard/stats` - Estadísticas generales
- `GET /api/dashboard/usage-by-branch` - Uso por sucursal
- `GET /api/dashboard/usage-by-classroom` - Uso por aula
- `GET /api/dashboard/active-sessions-summary` - Resumen sesiones activas

### Auditoría
- `GET /api/audit` - Listar logs de auditoría
- `GET /api/audit/{id}` - Obtener log específico

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UnilockerDBV1;User Id=sa;Password=tu_password;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "ClaveSecreta32CaracteresMinimo!!!",
    "Issuer": "UnilockerAPI",
    "Audience": "UnilockerClients",
    "ExpirationMinutes": 480
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "correo@ejemplo.com",
    "SenderName": "Unilocker System",
    "Password": "contraseña_app"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Variables de Entorno (Producción)

```bash
# Cadena de conexión
export ConnectionStrings__DefaultConnection="Server=..."

# JWT
export Jwt__Key="ClaveSecretaMuyLargaYSegura..."

# Email
export Email__Password="password_app"
```

## Instalación

### Requisitos Previos
- .NET 8 SDK
- SQL Server 2019+
- Base de datos UnilockerDBV1 creada

### Pasos

1. **Clonar o descargar el proyecto**

2. **Restaurar paquetes NuGet**
```bash
cd Unilocker.Api
dotnet restore
```

3. **Configurar cadena de conexión**
Editar `appsettings.json` y actualizar `ConnectionStrings:DefaultConnection`

4. **Configurar JWT Key**
Cambiar `Jwt:Key` por una clave segura de al menos 32 caracteres

5. **Ejecutar la aplicación**
```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5013`
- HTTPS: `https://localhost:7198`

## Desarrollo

### Ejecutar en modo desarrollo
```bash
dotnet watch run
```

### Compilar para producción
```bash
dotnet publish -c Release -o ./publish
```

### Ejecutar pruebas
```bash
dotnet test
```

## Autenticación

### Obtener Token JWT

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "contraseña"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "usuario",
  "roleId": 1,
  "roleName": "Administrador",
  "expiresAt": "2025-12-17T20:00:00Z"
}
```

### Usar Token en Requests

```http
GET /api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Autenticación 2FA

### 1. Solicitar código 2FA
```http
POST /api/auth/request-2fa
Content-Type: application/json

{
  "email": "usuario@ejemplo.com"
}
```

### 2. Verificar código
```http
POST /api/auth/verify-2fa
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "contraseña",
  "twoFactorCode": "123456"
}
```

## Sistema de Roles

### Roles Disponibles
1. **Administrador (RoleId: 1)**
   - Acceso completo al sistema
   - Gestión de usuarios y configuración
   - Visualización de auditoría

2. **Docente (RoleId: 2)**
   - Gestión de sesiones
   - Gestión de reportes
   - Visualización de estadísticas

3. **Estudiante (RoleId: 3)**
   - Inicio/fin de sesión
   - Creación de reportes
   - Visualización de sesiones propias

### Verificación de Roles

Los controladores usan el atributo `[Authorize]` para proteger endpoints:

```csharp
[Authorize] // Requiere autenticación
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Métodos protegidos
}
```

## Auditoría

Todas las operaciones importantes se registran automáticamente en la tabla `AuditLog`:

- Usuario que realizó la acción
- IP del cliente
- Timestamp
- Acción realizada
- Detalles de la operación

**Ejemplo de log:**
```json
{
  "id": 1,
  "userId": 5,
  "action": "Crear Usuario",
  "details": "Usuario creado: juan.perez",
  "ipAddress": "192.168.1.100",
  "timestamp": "2025-12-17T15:30:00"
}
```

## Eliminación Lógica

El sistema implementa eliminación lógica mediante el campo `Status`:
- `Status = true`: Registro activo
- `Status = false`: Registro eliminado

**Ejemplo:**
```csharp
// En lugar de eliminar físicamente:
computer.Status = false;
await _context.SaveChangesAsync();

// Los queries filtran automáticamente:
var activeComputers = await _context.Computers
    .Where(c => c.Status == true)
    .ToListAsync();
```

## Gestión de Sesiones

### Flujo de Sesión

1. **Inicio de Sesión**
   - Cliente envía `POST /api/sessions/start`
   - Se crea registro en tabla `Session` con `IsActive = true`
   - Se retorna información de la sesión

2. **Mantener Sesión Activa (Heartbeat)**
   - Cliente envía `PUT /api/sessions/{id}/heartbeat` cada 30 segundos
   - Actualiza campo `LastHeartbeat`

3. **Fin de Sesión**
   - Cliente envía `PUT /api/sessions/{id}/end`
   - Se establece `EndDateTime` e `IsActive = false`
   - Se calcula duración total

### Verificación de Sesiones Expiradas

Las sesiones se consideran expiradas si:
- `LastHeartbeat` tiene más de 2 minutos sin actualizar
- Usuario cerró aplicación sin finalizar sesión

## Validación de Datos

La API usa DataAnnotations para validación automática:

```csharp
public class StartSessionRequest
{
    [Required(ErrorMessage = "UserId es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "ComputerId es requerido")]
    public int ComputerId { get; set; }
}
```

## Manejo de Errores

Respuestas de error estandarizadas:

```json
{
  "message": "Descripción del error",
  "error": "Detalles técnicos (solo en desarrollo)"
}
```

Códigos HTTP utilizados:
- `200 OK`: Operación exitosa
- `201 Created`: Recurso creado
- `400 Bad Request`: Datos inválidos
- `401 Unauthorized`: No autenticado
- `403 Forbidden`: Sin permisos
- `404 Not Found`: Recurso no encontrado
- `409 Conflict`: Conflicto (ej. sesión ya activa)
- `500 Internal Server Error`: Error del servidor

## Seguridad

### Contraseñas
- Hasheadas con BCrypt (factor de trabajo: 12)
- Nunca se almacenan en texto plano
- Validación de longitud mínima

### Tokens JWT
- Firmados con clave secreta
- Expiración configurable (por defecto 8 horas)
- Incluyen información del usuario y rol

### CORS
Configurado para aceptar peticiones desde:
- Cliente WPF (localhost)
- Dashboard Web (localhost)
- Producción (configurar dominios específicos)

### SQL Injection
- Protección mediante Entity Framework
- Queries parametrizadas

## Logs

Los logs se generan automáticamente y se muestran en consola:

```
info: Unilocker.Api.Controllers.SessionsController[0]
      Iniciando sesión - UserId: 3, ComputerId: 15
info: Unilocker.Api.Controllers.SessionsController[0]
      Sesión creada exitosamente - SessionId: 42
```

## Dependencias (NuGet)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## Solución de Problemas

### Error de conexión a base de datos
- Verificar que SQL Server esté ejecutándose
- Confirmar credenciales en `appsettings.json`
- Verificar nombre de base de datos

### Error de autenticación JWT
- Verificar que `Jwt:Key` tenga al menos 32 caracteres
- Confirmar que el token no haya expirado
- Verificar formato del header: `Authorization: Bearer {token}`

### Email 2FA no se envía
- Verificar configuración SMTP
- Usar contraseña de aplicación (no contraseña regular)
- Confirmar que el puerto 587 esté abierto

## Contacto

Para soporte o consultas:
- Email: ghr0034560@est.univalle.edu
- Autor: Rodrigo Gutierrez Herrera

---

**Versión:** 1.0.0  
**Última actualización:** Diciembre 2025
