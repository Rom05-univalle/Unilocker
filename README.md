# 🔐 Unilocker - Sistema de Control de Acceso a Laboratorios

Sistema integral para la gestión y control de acceso a laboratorios de cómputo en instituciones educativas.

## 📋 Descripción del Proyecto

Unilocker es un sistema de tres componentes que permite:
- **Control de acceso** a computadoras en laboratorios mediante inicio de sesión
- **Monitoreo en tiempo real** de sesiones activas
- **Gestión de reportes** de problemas técnicos
- **Auditoría completa** de todas las acciones del sistema

## 🏗️ Arquitectura del Sistema

```
┌─────────────────┐      ┌──────────────────┐      ┌─────────────────┐
│  Cliente WPF    │ ────▶│   API REST       │ ────▶│   SQL Server    │
│  (.NET 8)       │      │   (.NET 8)       │      │   Database      │
└─────────────────┘      └──────────────────┘      └─────────────────┘
                                   ▲
                                   │
                         ┌─────────┴──────────┐
                         │  Web Dashboard     │
                         │  (HTML/CSS/JS)     │
                         └────────────────────┘
```

## 📂 Estructura del Proyecto

```
UnilockerProyecto/
├── Unilocker.Api/          # API REST Backend (.NET 8)
├── Unilocker.Client/       # Cliente de escritorio WPF
├── Unilocker.Web/          # Dashboard web para administración
├── Database/               # Scripts SQL de base de datos
├── installer/              # Instalador del cliente
│   ├── UnilockerInstaller.iss
│   └── UnilockerClientSetup_v1.0.0.exe
└── README.md
```

## 🚀 Componentes

### 1. Cliente WPF (Unilocker.Client)
Aplicación de escritorio que se ejecuta en cada computadora del laboratorio:
- **Modo Kiosco**: Bloquea el equipo hasta iniciar sesión
- **Auto-inicio**: Se ejecuta automáticamente al encender el equipo
- **Registro de equipos**: Configuración inicial con UUID único
- **Reportes**: Los usuarios pueden reportar problemas
- **Heartbeat**: Mantiene sesión activa con verificaciones periódicas

**Tecnologías:**
- .NET 8 WPF
- Material Design
- BCrypt para seguridad

### 2. API REST (Unilocker.Api)
Backend que centraliza toda la lógica de negocio:
- **Autenticación JWT** con 2FA opcional
- **CRUD completo** para todas las entidades
- **Control de sesiones** activas
- **Gestión de reportes** de problemas
- **Auditoría automática** de todas las operaciones
- **Endpoints RESTful** documentados

**Tecnologías:**
- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt

### 3. Dashboard Web (Unilocker.Web)
Interfaz administrativa para gestión del sistema:
- **Dashboard visual** con estadísticas en tiempo real
- **Gestión de usuarios** y roles
- **Administración de infraestructura** (Sedes, Bloques, Aulas)
- **Monitoreo de sesiones** activas
- **Gestión de reportes** y problemas
- **Visualización de auditoría**

**Tecnologías:**
- HTML5, CSS3, JavaScript (ES6+)
- Bootstrap 5
- Chart.js para gráficos
- Fetch API

## 💾 Base de Datos

### Esquema Principal
- **Roles**: Administrador, Usuario, Supervisor
- **Users**: Usuarios del sistema con autenticación
- **Branches**: Sedes universitarias
- **Blocks**: Bloques/Edificios
- **Classrooms**: Aulas/Laboratorios
- **Computers**: Computadoras registradas
- **Sessions**: Sesiones activas e históricas
- **Reports**: Reportes de problemas
- **ProblemTypes**: Categorías de problemas
- **AuditLogs**: Registro de auditoría

Ver detalles en [Database/README.md](Database/README.md)

## 🔧 Instalación y Configuración

### Requisitos Previos
- Windows 10/11
- .NET 8 SDK
- SQL Server 2019+
- Visual Studio 2022 (opcional)

### 1. Base de Datos
```bash
cd Database
sqlcmd -S localhost -i 01_CREATE_DATABASE.sql
sqlcmd -S localhost -i 02_INSERT_DATA.sql
```

### 2. API Backend
```bash
cd Unilocker.Api
# Configurar appsettings.json con cadena de conexión
dotnet run
```
La API estará disponible en `http://localhost:5013`

### 3. Cliente WPF
**Opción A: Usar instalador**
```bash
cd installer
.\UnilockerClientSetup_v1.0.0.exe
```

**Opción B: Compilar desde código**
```bash
cd Unilocker.Client
dotnet run
```

### 4. Dashboard Web
Abrir `Unilocker.Web/index.html` con Live Server o cualquier servidor web local.

## 📦 Instalador

El instalador automático incluye:
- ✅ Instalación del cliente en Program Files
- ✅ Configuración de URL de API durante instalación
- ✅ Auto-inicio de Windows (opcional)
- ✅ Acceso directo en escritorio
- ✅ Desinstalador completo

**Ubicación:** `installer/UnilockerClientSetup_v1.0.0.exe`

## 👥 Usuarios de Prueba

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| radmin | admin123 | Administrador |
| usuario1 | password123 | Usuario |
| usuario2 | password123 | Usuario |

## 🔒 Seguridad

- **Autenticación JWT** con tokens seguros
- **Contraseñas hasheadas** con BCrypt
- **2FA opcional** vía correo electrónico
- **Auditoría completa** de todas las acciones
- **Modo Kiosco** que previene bypass del sistema
- **UUIDs únicos** para cada equipo

## 🎯 Características Principales

### Modo Kiosco
- Bloquea el cierre de la aplicación hasta login exitoso
- Permite cierre con Alt+F4 solo si hay problemas de conexión
- Se minimiza después del login (no se puede cerrar)

### Control de Sesiones
- Inicio/fin automático de sesiones
- Heartbeat cada 30 segundos para mantener sesión activa
- Cierre automático de sesión al cerrar aplicación
- Historial completo de sesiones

### Sistema de Reportes
- Los usuarios pueden reportar problemas
- Categorización por tipo de problema
- Estados: Pendiente, En Proceso, Resuelto
- Tracking completo con auditoría

### Auditoría
- Registro automático de todas las acciones
- Información de usuario, IP, timestamp
- Detalles de la operación realizada
- Visualización en dashboard web

## 🛠️ Desarrollo

### Compilar Cliente
```bash
cd Unilocker.Client
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Compilar API
```bash
cd Unilocker.Api
dotnet publish -c Release
```

### Generar Instalador
Usar Inno Setup con el script `UnilockerInstaller.iss`

## 📝 Licencia

Proyecto académico - Universidad del Valle

## 👨‍💻 Autor

Rom05-univalle

## 📧 Contacto

ghr0034560@est.univalle.edu

---

## ⚙️ Personalización y Configuración

### Configuración del Cliente (Unilocker.Client)

El archivo `appsettings.json` permite personalizar el comportamiento del cliente:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5013"  // URL del servidor API
  },
  "AppSettings": {
    "DataDirectory": "C:\\ProgramData\\Unilocker",  // Directorio de datos
    "MachineIdFile": "machine.id",                   // Archivo UUID del equipo
    "RegisteredFlagFile": "registered.flag"          // Bandera de registro
  }
}
```

**Parámetros Configurables:**
- `BaseUrl`: URL del servidor API (se configura automáticamente durante la instalación)
- `DataDirectory`: Ubicación donde se almacenan datos locales (IDs, flags)
- `MachineIdFile`: Nombre del archivo que contiene el UUID único del equipo
- `RegisteredFlagFile`: Archivo que indica si el equipo está registrado

**Configuración de Heartbeat:**
- Intervalo por defecto: 30 segundos
- Modificable en `MainWindow.xaml.cs` → `_heartbeatTimer.Interval`

**Modo Kiosco:**
- Habilitado por defecto en `LoginWindow`
- Para deshabilitar: Modificar `AllowsTransparency`, `WindowStyle` y `Topmost` en `LoginWindow.xaml`

### Configuración del API (Unilocker.Api)

El archivo `appsettings.json` controla todos los aspectos del servidor:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVER;Database=DB;User Id=USER;Password=PASS;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "ClaveSecreta32CaracteresMinimo!!!",
    "Issuer": "UnilockerAPI",
    "Audience": "UnilockerClients",
    "ExpirationMinutes": 480  // Duración del token (8 horas)
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "correo@ejemplo.com",
    "SenderName": "Unilocker System",
    "Password": "contraseña_app"  // App password de Gmail
  }
}
```

**Parámetros de Base de Datos:**
- `Server`: Dirección del servidor SQL Server
- `Database`: Nombre de la base de datos
- `User Id`: Usuario de SQL Server
- `Password`: Contraseña del usuario
- `TrustServerCertificate`: Aceptar certificados autofirmados

**Configuración JWT:**
- `Key`: Clave secreta (mínimo 32 caracteres, cambiar en producción)
- `Issuer`: Identificador del emisor de tokens
- `Audience`: Audiencia válida para los tokens
- `ExpirationMinutes`: Tiempo de vida del token (480 = 8 horas)

**Configuración de Email (2FA):**
- `SmtpServer`: Servidor SMTP para envío de correos
- `SmtpPort`: Puerto SMTP (587 para TLS)
- `SenderEmail`: Correo emisor
- `Password`: Contraseña de aplicación (no la contraseña regular)

**Variables de Entorno de Producción:**
```bash
# Recomendado: Usar variables de entorno para datos sensibles
export ConnectionStrings__DefaultConnection="Server=..."
export Jwt__Key="ClaveSecretaMuyLarga..."
export Email__Password="password_app"
```

### Configuración del Dashboard Web

Modificar `Unilocker.Web/js/config.js` (o directamente en cada archivo JS):

```javascript
const API_BASE_URL = 'http://localhost:5013/api';
```

**Cambiar para producción:**
```javascript
const API_BASE_URL = 'http://192.168.1.100:5013/api';  // IP del servidor
```

### Personalización de Roles y Permisos

Los roles se definen en la base de datos (`Roles` table):
- **Administrador**: Acceso completo al sistema
- **Usuario**: Acceso limitado (login, reportes)
- **Supervisor**: Acceso a monitoreo y reportes

**Para crear nuevos roles:**
```sql
INSERT INTO Roles (Name, Description) 
VALUES ('NuevoRol', 'Descripción del rol');
```

**Para asignar roles a usuarios:**
```sql
UPDATE Users SET RoleId = (SELECT Id FROM Roles WHERE Name = 'NuevoRol')
WHERE Email = 'usuario@ejemplo.com';
```

### Personalización de Tipos de Problemas

Agregar nuevos tipos de problemas técnicos:

```sql
INSERT INTO ProblemTypes (Name, Description, IsActive) 
VALUES ('Nuevo Problema', 'Descripción detallada', 1);
```

---

## 🔒 Seguridad

### Autenticación y Autorización

**Sistema de Autenticación:**
- **JWT Tokens**: Autenticación basada en tokens con expiración configurable
- **BCrypt Hashing**: Contraseñas hasheadas con BCrypt (factor de trabajo: 12)
- **2FA Opcional**: Autenticación de dos factores vía email
- **Validación de Tokens**: Todos los endpoints protegidos requieren token válido

**Endpoints Protegidos:**
Todos los controladores de la API están protegidos con `[Authorize]`:
- `/api/users` - Gestión de usuarios
- `/api/sessions` - Control de sesiones
- `/api/reports` - Gestión de reportes
- `/api/branches`, `/api/blocks`, `/api/classrooms` - Infraestructura
- `/api/dashboard` - Estadísticas
- `/api/audit` - Auditoría

**Endpoints Públicos (sin autenticación):**
- `POST /api/auth/login` - Inicio de sesión
- `POST /api/auth/request-2fa` - Solicitud de código 2FA
- `POST /api/auth/verify-2fa` - Verificación de código 2FA
- `POST /api/computers/register` - Registro inicial de equipos

### Gestión de Contraseñas

**Requisitos de Contraseñas:**
- Mínimo 8 caracteres
- Almacenamiento con BCrypt (factor de trabajo: 12)
- No se almacenan en texto plano

**Cambio de Contraseñas:**
```csharp
// En la API: UsersController.cs
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
```

**Verificación:**
```csharp
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

### Permisos y Control de Acceso

**Roles del Sistema:**
1. **Administrador**
   - Acceso completo al dashboard web
   - Gestión de usuarios, roles, infraestructura
   - Visualización de auditoría
   - Desregistro de equipos desde el cliente

2. **Usuario**
   - Login en equipos del laboratorio
   - Crear reportes de problemas
   - Ver sesión personal

3. **Supervisor**
   - Monitoreo de sesiones activas
   - Gestión de reportes
   - Visualización de estadísticas

**Verificación de Roles en el Cliente:**
```csharp
// MainWindow.xaml.cs
private void CheckAdminRole()
{
    if (userRole.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
    {
        BtnUnregister.Visibility = Visibility.Visible;  // Mostrar botón
    }
}
```

### Auditoría y Logs

**Sistema de Auditoría Automática:**
Todas las operaciones críticas se registran en `AuditLogs`:
- Usuario que realizó la acción
- Tipo de operación (Login, Logout, Create, Update, Delete)
- Entidad afectada
- Detalles de la operación
- Timestamp
- Dirección IP del cliente

**Operaciones Auditadas:**
- Inicio/cierre de sesión
- Registro/desregistro de equipos
- Creación/modificación de usuarios
- Gestión de infraestructura
- Creación/actualización de reportes

**Consulta de Auditoría:**
```sql
SELECT * FROM AuditLogs 
WHERE UserId = @userId 
ORDER BY CreatedAt DESC;
```

### Seguridad de Red

**Recomendaciones de Despliegue:**

1. **HTTPS en Producción:**
```json
// appsettings.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "cert_password"
        }
      }
    }
  }
}
```

2. **CORS (Cross-Origin Resource Sharing):**
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebDashboard",
        policy => policy.WithOrigins("http://localhost:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});
```

3. **Firewall:**
- Abrir solo el puerto necesario (5013 o el configurado)
- Limitar acceso a red local de laboratorios
- Usar VPN para acceso remoto

4. **SQL Server:**
- Usar usuario específico con permisos limitados
- No usar cuenta `sa`
- Habilitar SQL Server Authentication
- Configurar firewall de SQL Server

### Protección de Datos Sensibles

**Información Sensible:**
- Contraseñas: Hasheadas con BCrypt
- JWT Key: Almacenar en variables de entorno
- Connection Strings: Usar Azure Key Vault o variables de entorno
- Email Password: App Password de Gmail, no contraseña real

**Buenas Prácticas:**
```bash
# .env (no incluir en Git)
JWT_KEY=ClaveSecretaMuyLarga32CaracteresMinimo
DB_PASSWORD=ContraseñaSegura123!
EMAIL_PASSWORD=app_password_gmail
```

### Modo Kiosco - Seguridad Física

**Prevención de Bypass:**
- `WindowStyle.None` - Sin bordes para minimizar/cerrar
- `Topmost = true` - Ventana siempre en primer plano
- `ShowInTaskbar = false` - No visible en barra de tareas
- Control de `Window_Closing` - Previene cierre no autorizado

**Salida de Emergencia:**
- Alt+F4 permitido solo cuando `_hasConnectionIssue = true`
- Confirmación obligatoria antes de cerrar

### Recomendaciones Adicionales

1. **Actualización Regular:**
   - Mantener .NET 8 actualizado
   - Actualizar paquetes NuGet regularmente
   - Revisar vulnerabilidades conocidas

2. **Backup:**
   - Backup diario de base de datos
   - Backup de archivos de configuración
   - Plan de recuperación ante desastres

3. **Monitoreo:**
   - Revisar logs de auditoría regularmente
   - Monitorear sesiones activas
   - Alertas de intentos de login fallidos

4. **Capacitación:**
   - Usuarios: Procedimientos de login y reporte
   - Administradores: Gestión de roles y permisos
   - Supervisores: Monitoreo y resolución de reportes

---

## 🐛 Depuración y Solución de Problemas

### Problemas Comunes del Cliente

#### 1. **Error: "No se pudo conectar con el servidor"**

**Causa:** API no está accesible o URL incorrecta

**Solución:**
```bash
# Verificar que la API esté corriendo
cd Unilocker.Api
dotnet run

# Verificar URL en appsettings.json del cliente
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5013"  # Verificar IP/puerto correcto
  }
}

# Probar conexión manualmente
curl http://localhost:5013/api/health
```

**Mensajes Amigables:**
- "No se pudo conectar con el servidor" → API apagada o URL incorrecta
- "Usuario o contraseña incorrectos" → Credenciales inválidas (401)
- "Por favor verifica los datos ingresados" → Datos mal formateados (400)

#### 2. **Botón "Desregistrar Equipo" No Aparece**

**Causa:** Usuario no tiene rol "Administrador"

**Solución:**
```sql
-- Verificar rol del usuario
SELECT u.Name, r.Name as Role 
FROM Users u 
JOIN Roles r ON u.RoleId = r.Id 
WHERE u.Email = 'usuario@ejemplo.com';

-- Asignar rol Administrador
UPDATE Users 
SET RoleId = (SELECT Id FROM Roles WHERE Name = 'Administrador')
WHERE Email = 'usuario@ejemplo.com';
```

**Código de verificación:** `MainWindow.xaml.cs` línea 326
```csharp
userRole.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
```

#### 3. **Nombre de Equipo Muestra "equipo registrado"**

**Causa:** Versión antigua del cliente

**Solución:**
- Reinstalar con el instalador actualizado (`UnilockerClientSetup_v1.0.0.exe`)
- El nuevo cliente guarda el nombre en `C:\ProgramData\Unilocker\computer_name.dat`

**Verificar:**
```powershell
Get-Content "C:\ProgramData\Unilocker\computer_name.dat"
```

#### 4. **No se Puede Cerrar la Aplicación**

**Causa:** Modo Kiosco activo

**Solución Esperada:**
- No se puede cerrar hasta completar login (comportamiento diseñado)
- Después del login, se puede minimizar pero no cerrar
- Alt+F4 funciona solo si hay error de conexión con API

**Salida de Emergencia:**
1. Detener API para simular error de conexión
2. Presionar Alt+F4
3. Confirmar cierre

#### 5. **Error: "El equipo ya está registrado"**

**Causa:** UUID ya existe en base de datos

**Solución:**
```sql
-- Verificar registro
SELECT * FROM Computers WHERE UUID = 'uuid-del-equipo';

-- Opción 1: Desregistrar desde el cliente (con rol Administrador)
-- Opción 2: Eliminar desde base de datos
DELETE FROM Sessions WHERE ComputerId = @computerId;
DELETE FROM Computers WHERE UUID = 'uuid-del-equipo';

-- Opción 3: Actualizar nombre del equipo existente
UPDATE Computers SET Name = 'NuevoNombre' WHERE UUID = 'uuid-del-equipo';
```

### Problemas Comunes de la API

#### 1. **Error de Conexión a Base de Datos**

**Mensajes:**
```
SqlException: Cannot open database
A network-related error occurred
```

**Solución:**
```bash
# Verificar servicio SQL Server
Get-Service MSSQL* | Where-Object {$_.Status -eq 'Running'}

# Probar conexión
sqlcmd -S DESKTOP-C82PFDH\SQLEXPRESS -U Unilocker_Access -P Uni2025!SecurePass

# Verificar cadena de conexión en appsettings.json
"Server=DESKTOP-C82PFDH\\SQLEXPRESS,1433;Database=UnilockerDBV1;..."

# Habilitar TCP/IP en SQL Server Configuration Manager
# Abrir puerto 1433 en Firewall de Windows
```

#### 2. **Error 401 Unauthorized en Endpoints**

**Causa:** Token JWT inválido o expirado

**Solución:**
```javascript
// Verificar token en LocalStorage (Dashboard Web)
console.log(localStorage.getItem('token'));

// Re-login para obtener nuevo token
// Token expira según ExpirationMinutes (480 = 8 horas)

// Verificar headers en peticiones
fetch(API_URL, {
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    }
});
```

#### 3. **CORS Error en Dashboard Web**

**Mensaje:**
```
Access to fetch at 'http://localhost:5013' from origin 'http://localhost:5500' 
has been blocked by CORS policy
```

**Solución:**
```csharp
// Program.cs - Agregar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

app.UseCors("AllowAll");
```

#### 4. **Error de Migración de Base de Datos**

**Causa:** Cambios en modelos no reflejados en BD

**Solución:**
```bash
cd Unilocker.Api

# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migración
dotnet ef database update

# Revertir migración
dotnet ef database update AnteriorMigracion

# Eliminar última migración
dotnet ef migrations remove
```

### Problemas del Dashboard Web

#### 1. **Datos No Cargan en Dashboard**

**Causa:** API no responde o token inválido

**Solución:**
```javascript
// Abrir DevTools (F12) → Console
// Verificar errores de red

// Probar endpoint manualmente
fetch('http://localhost:5013/api/dashboard/stats', {
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
})
.then(r => r.json())
.then(console.log);

// Verificar que la API esté corriendo
// Verificar que la URL en config.js sea correcta
```

#### 2. **Gráficos No Se Muestran**

**Causa:** Chart.js no cargado o datos incorrectos

**Solución:**
```html
<!-- Verificar que Chart.js esté incluido -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- Verificar consola de errores -->
<!-- Verificar formato de datos -->
```

### Conflictos con Otros Sistemas

#### 1. **Puerto 5013 en Uso**

**Solución:**
```bash
# Verificar qué proceso usa el puerto
netstat -ano | findstr :5013

# Matar proceso
taskkill /PID [pid] /F

# Cambiar puerto en launchSettings.json y appsettings del cliente
"applicationUrl": "http://localhost:5014"
```

#### 2. **Conflicto con Antivirus**

**Síntoma:** Instalador bloqueado o ejecutable no inicia

**Solución:**
- Agregar excepción en Windows Defender
- Firmar ejecutable con certificado digital
- Ejecutar como administrador

#### 3. **Permisos de Carpeta**

**Error:** `UnauthorizedAccessException` al acceder a `C:\ProgramData\Unilocker`

**Solución:**
```powershell
# Crear carpeta con permisos correctos
New-Item -Path "C:\ProgramData\Unilocker" -ItemType Directory -Force
icacls "C:\ProgramData\Unilocker" /grant "Users:(OI)(CI)F" /T
```

### Herramientas de Diagnóstico

#### Logs de la API
```bash
# Ver logs en tiempo real
dotnet run --verbosity detailed

# Logs de Entity Framework
# Agregar en appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### Debugging del Cliente WPF
```bash
# Ejecutar en modo debug
dotnet run --configuration Debug

# Ver output en Visual Studio
# Debug → Windows → Output
```

#### Inspección de Base de Datos
```sql
-- Verificar sesiones activas
SELECT s.*, u.Name as UserName, c.Name as ComputerName
FROM Sessions s
JOIN Users u ON s.UserId = u.Id
JOIN Computers c ON s.ComputerId = c.Id
WHERE s.EndTime IS NULL;

-- Ver últimos registros de auditoría
SELECT TOP 20 * FROM AuditLogs ORDER BY CreatedAt DESC;

-- Equipos registrados
SELECT * FROM Computers WHERE IsActive = 1;
```

### Contacto de Soporte

Si los problemas persisten:
- Email: ghr0034560@est.univalle.edu
- Revisar logs de auditoría en el dashboard
- Consultar documentación de Base de Datos (`Database/README.md`)

---

## 📚 Glosario de Términos

### Términos Técnicos

**API (Application Programming Interface)**
- Interfaz que permite la comunicación entre diferentes sistemas de software
- En Unilocker: Backend REST que centraliza la lógica de negocio

**BCrypt**
- Algoritmo de hashing de contraseñas basado en Blowfish
- Factor de trabajo: 12 (número de iteraciones, mayor = más seguro)

**CORS (Cross-Origin Resource Sharing)**
- Mecanismo de seguridad que permite peticiones HTTP entre diferentes dominios
- Necesario para que el dashboard web acceda a la API

**CRUD (Create, Read, Update, Delete)**
- Operaciones básicas de persistencia de datos
- Aplicado a todas las entidades del sistema

**DTO (Data Transfer Object)**
- Objeto simple usado para transferir datos entre subsistemas
- Ejemplo: `ComputerResponse`, `RegisterComputerRequest`

**Entity Framework Core**
- ORM (Object-Relational Mapper) para .NET
- Mapea clases C# a tablas de base de datos

**Heartbeat**
- Señal periódica que indica que un sistema está activo
- En Unilocker: Cliente envía ping cada 30 segundos para mantener sesión

**JWT (JSON Web Token)**
- Estándar abierto para transmitir información de forma segura
- Usado para autenticación en la API (no requiere sesiones en servidor)

**ORM (Object-Relational Mapping)**
- Técnica para convertir datos entre sistemas incompatibles (objetos ↔ tablas)
- Entity Framework Core es el ORM usado en Unilocker

**REST (Representational State Transfer)**
- Estilo arquitectónico para diseñar servicios web
- Usa HTTP methods: GET, POST, PUT, DELETE

**2FA (Two-Factor Authentication)**
- Autenticación de dos pasos (contraseña + código temporal)
- Código enviado por email en Unilocker

**UUID (Universally Unique Identifier)**
- Identificador único de 128 bits
- Cada equipo tiene un UUID basado en hardware (CPUID + MAC)

**WPF (Windows Presentation Foundation)**
- Framework de Microsoft para crear interfaces de usuario en Windows
- Usado en el cliente de escritorio

### Términos del Dominio

**Auditoría (Audit Log)**
- Registro de todas las acciones realizadas en el sistema
- Incluye: usuario, acción, timestamp, IP

**Bloque (Block)**
- Edificio o sección dentro de una sede universitaria
- Ejemplo: "Bloque A", "Edificio Administrativo"

**Aula/Laboratorio (Classroom)**
- Sala específica dentro de un bloque
- Ejemplo: "Lab 301", "Sala de Cómputo 1"

**Computadora (Computer)**
- Equipo físico registrado en el sistema
- Identificado por UUID único

**Modo Kiosco (Kiosk Mode)**
- Configuración que bloquea el equipo hasta iniciar sesión
- Previene uso no autorizado de computadoras

**Reporte (Report)**
- Problema técnico reportado por un usuario
- Estados: Pendiente, En Proceso, Resuelto

**Rol (Role)**
- Conjunto de permisos asignados a usuarios
- Roles: Administrador, Usuario, Supervisor

**Sede (Branch)**
- Campus o ubicación física de la institución
- Ejemplo: "Sede Central", "Sede Norte"

**Sesión (Session)**
- Período de uso de una computadora por un usuario
- Inicio automático al login, fin al logout

**Tipo de Problema (Problem Type)**
- Categoría de problema técnico
- Ejemplos: "Hardware", "Software", "Red"

### Siglas y Abreviaciones

**API** - Application Programming Interface  
**CRUD** - Create, Read, Update, Delete  
**CORS** - Cross-Origin Resource Sharing  
**DTO** - Data Transfer Object  
**EF Core** - Entity Framework Core  
**HTTP** - Hypertext Transfer Protocol  
**HTTPS** - HTTP Secure  
**JWT** - JSON Web Token  
**ORM** - Object-Relational Mapping  
**REST** - Representational State Transfer  
**SQL** - Structured Query Language  
**TLS** - Transport Layer Security  
**UUID** - Universally Unique Identifier  
**WPF** - Windows Presentation Foundation  
**2FA** - Two-Factor Authentication  

### Conceptos de Negocio

**Desregistro de Equipo**
- Eliminación de un equipo del sistema
- Solo disponible para rol Administrador
- Borra UUID y flag de registro

**Registro de Equipo**
- Proceso inicial de agregar un equipo al sistema
- Genera UUID único basado en hardware
- Asigna nombre y aula

**Control de Acceso**
- Sistema que requiere autenticación para usar equipos
- Implementado mediante modo kiosco

**Monitoreo de Sesiones**
- Visualización de usuarios conectados en tiempo real
- Disponible en dashboard web

**Estadísticas**
- Métricas del sistema: sesiones activas, reportes pendientes, etc.
- Mostradas en dashboard

---

## 📖 Referencias y Recursos Adicionales

### Documentación Oficial

**.NET y C#**
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)

**Seguridad**
- [JWT.io - JSON Web Tokens](https://jwt.io/)
- [BCrypt Documentation](https://github.com/BcryptNet/bcrypt.net)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)

**Base de Datos**
- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [T-SQL Reference](https://learn.microsoft.com/en-us/sql/t-sql/)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

**Frontend**
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)
- [Chart.js Documentation](https://www.chartjs.org/docs/)
- [MDN Web Docs](https://developer.mozilla.org/)

### Tutoriales y Guías

**API REST con .NET**
- [Building RESTful APIs with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api)
- [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

**WPF**
- [WPF Tutorial](https://www.wpf-tutorial.com/)
- [Material Design in XAML](http://materialdesigninxaml.net/)
- [WPF MVVM Pattern](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)

**SQL Server**
- [SQL Server Tutorial](https://www.sqlservertutorial.net/)
- [Database Design Basics](https://learn.microsoft.com/en-us/office/troubleshoot/access/database-design-principles)

### Herramientas de Desarrollo

**IDEs y Editores**
- [Visual Studio 2022](https://visualstudio.microsoft.com/) - IDE principal para .NET
- [Visual Studio Code](https://code.visualstudio.com/) - Editor ligero
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/) - Gestión de BD

**Herramientas de Testing**
- [Postman](https://www.postman.com/) - Testing de APIs
- [Thunder Client](https://www.thunderclient.com/) - Testing en VS Code
- [curl](https://curl.se/) - Cliente HTTP de línea de comandos

**Utilidades**
- [Inno Setup](https://jrsoftware.org/isinfo.php) - Crear instaladores
- [Live Server](https://marketplace.visualstudio.com/items?itemName=ritwickdey.LiveServer) - Servidor web local
- [Git](https://git-scm.com/) - Control de versiones

### Librerías y Paquetes NuGet

**Backend (Unilocker.Api)**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.4.0" />
```

**Cliente (Unilocker.Client)**
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.4.0" />
<PackageReference Include="System.Management" Version="8.0.0" />
```

### Recursos de la Comunidad

**Stack Overflow**
- [.NET Tag](https://stackoverflow.com/questions/tagged/.net)
- [ASP.NET Core Tag](https://stackoverflow.com/questions/tagged/asp.net-core)
- [WPF Tag](https://stackoverflow.com/questions/tagged/wpf)
- [Entity Framework Tag](https://stackoverflow.com/questions/tagged/entity-framework-core)

**Reddit**
- [r/dotnet](https://www.reddit.com/r/dotnet/)
- [r/csharp](https://www.reddit.com/r/csharp/)
- [r/webdev](https://www.reddit.com/r/webdev/)

**Discord**
- [C# Discord](https://discord.gg/csharp)
- [.NET Discord](https://discord.gg/dotnet)

### Documentación del Proyecto

**Archivos Locales**
- `Database/README.md` - Documentación de base de datos
- `Database/01_CREATE_DATABASE.sql` - Script de creación
- `Database/02_INSERT_DATA.sql` - Datos de prueba
- `UnilockerInstaller.iss` - Script del instalador

### Artículos y Mejores Prácticas

**Arquitectura**
- [Clean Architecture in .NET](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [RESTful API Design Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)

**Seguridad**
- [OWASP API Security Top 10](https://owasp.org/API-Security/editions/2023/en/0x00-header/)
- [Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)

**Performance**
- [Entity Framework Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [ASP.NET Core Performance](https://learn.microsoft.com/en-us/aspnet/core/performance/)

### Soporte del Proyecto

**Repositorio GitHub**
- Owner: Rom05-univalle
- Repo: Unilocker
- Issues: Para reportar bugs o solicitar features

**Contacto Directo**
- Email: ghr0034560@est.univalle.edu
- Institución: Universidad Privada del Valle

### Videos y Cursos (Recomendados)

**YouTube Channels**
- [Microsoft Developer](https://www.youtube.com/@MicrosoftDeveloper)
- [dotnet](https://www.youtube.com/@dotnet)
- [IAmTimCorey](https://www.youtube.com/@IAmTimCorey)

**Plataformas de Aprendizaje**
- [Microsoft Learn](https://learn.microsoft.com/) - Gratis
- [Pluralsight](https://www.pluralsight.com/) - .NET courses
- [Udemy](https://www.udemy.com/) - ASP.NET Core courses

---

## 🛠️ Herramientas de Implementación

### Lenguajes de Programación

**C# 12.0**
- Lenguaje principal del proyecto
- Usado en: API (backend) y Cliente (desktop)
- Características usadas:
  - Async/Await para operaciones asíncronas
  - LINQ para consultas a base de datos
  - Nullable reference types
  - Record types para DTOs
  - Pattern matching

**JavaScript (ES6+)**
- Usado en: Dashboard Web
- Características usadas:
  - Async/Await para llamadas a API
  - Fetch API para peticiones HTTP
  - Template literals
  - Arrow functions
  - Destructuring

**T-SQL (Transact-SQL)**
- Lenguaje de base de datos
- Scripts de creación y migración
- Stored procedures (no utilizados actualmente)

**HTML5 / CSS3**
- Markup y estilos del dashboard web
- CSS Grid y Flexbox para layouts
- Media queries para responsividad

### Frameworks y Librerías

#### Backend (Unilocker.Api)

**ASP.NET Core 8.0**
- Framework web para la API REST
- Características utilizadas:
  - Minimal APIs (opcional)
  - Dependency Injection
  - Middleware pipeline
  - Authentication & Authorization
  - CORS

**Entity Framework Core 8.0**
- ORM para acceso a base de datos
- Code-First approach
- Migrations para versionado de esquema
- LINQ to Entities para queries

**BCrypt.Net-Next 4.0.3**
- Hashing de contraseñas
- Factor de trabajo: 12
- Salt automático

**System.IdentityModel.Tokens.Jwt 8.4.0**
- Generación y validación de tokens JWT
- Claims-based authentication

#### Cliente (Unilocker.Client)

**WPF (Windows Presentation Foundation)**
- Framework de UI para Windows
- XAML para definición de interfaces
- Data binding
- MVVM pattern (parcialmente implementado)

**Material Design in XAML (implícito)**
- Estilos visuales modernos
- Componentes UI consistentes

**Newtonsoft.Json 13.0.3**
- Serialización/deserialización JSON
- Usado para comunicación con API

**System.Management 8.0.0**
- Acceso a información de hardware
- Generación de UUID basado en CPUID y MAC address

#### Frontend (Unilocker.Web)

**Bootstrap 5.3**
- Framework CSS para diseño responsivo
- Componentes pre-diseñados
- Grid system

**Chart.js 4.x**
- Gráficos y visualizaciones
- Usado en dashboard para estadísticas
- Gráficos de línea, barras, dona

**Vanilla JavaScript**
- No se usan frameworks pesados (React, Vue, Angular)
- DOM manipulation nativa
- Fetch API para peticiones

### APIs y Servicios de Terceros

**SQL Server Express**
- Base de datos relacional
- Versión: 2019 o superior
- Gratis para desarrollo/producción limitada

**Gmail SMTP (Opcional)**
- Servicio para envío de emails 2FA
- Configuración:
  - Server: smtp.gmail.com
  - Port: 587 (TLS)
  - Requiere "App Password"

### Base de Datos

**Microsoft SQL Server**
- Sistema de gestión de base de datos
- Características utilizadas:
  - Relaciones con Foreign Keys
  - Índices para performance
  - Transacciones ACID
  - Constraints (CHECK, UNIQUE, NOT NULL)

**Esquema:**
- 10 tablas principales
- Relaciones uno-a-muchos
- Integridad referencial
- Datos de auditoría (CreatedAt, UpdatedAt)

### Herramientas de Desarrollo

**Visual Studio 2022**
- IDE principal para desarrollo .NET
- Debugging integrado
- NuGet Package Manager
- Entity Framework Tools

**Visual Studio Code**
- Editor para dashboard web
- Extensiones:
  - Live Server
  - ESLint
  - Prettier

**SQL Server Management Studio (SSMS)**
- Gestión de base de datos
- Ejecución de scripts
- Visualización de datos

**Git / GitHub**
- Control de versiones
- Repositorio: Rom05-univalle/Unilocker
- Branch principal: main

**Inno Setup 6**
- Creador de instaladores para Windows
- Script: `UnilockerInstaller.iss`
- Compresión LZMA2

### Arquitectura y Patrones

**Arquitectura en 3 Capas**
```
┌─────────────────────────────────────┐
│    Presentation Layer (UI)           │
│  - WPF Client                        │
│  - Web Dashboard                     │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│    Business Logic Layer (API)        │
│  - Controllers                       │
│  - Services (implícito)              │
│  - DTOs                              │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│    Data Access Layer                 │
│  - Entity Framework Core             │
│  - DbContext                         │
│  - Models                            │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│    Database (SQL Server)             │
└─────────────────────────────────────┘
```

**Patrones de Diseño Utilizados**

1. **Repository Pattern** (implícito en EF Core)
   - DbContext actúa como Unit of Work
   - DbSet<T> como repositorios

2. **DTO Pattern** (Data Transfer Objects)
   - `ComputerResponse`
   - `RegisterComputerRequest`
   - `ClassroomInfo`
   - Separación entre modelos de BD y API

3. **Dependency Injection**
   - DbContext inyectado en controladores
   - IConfiguration para settings
   - Scoped lifetime para DbContext

4. **Middleware Pipeline**
   - Authentication
   - Authorization
   - CORS
   - Exception handling

5. **RESTful Architecture**
   - Recursos identificados por URLs
   - HTTP methods (GET, POST, PUT, DELETE)
   - Stateless communication
   - JSON como formato de datos

### Protocolos y Estándares

**HTTP/HTTPS**
- Protocolo de comunicación cliente-servidor
- Desarrollo: HTTP (localhost)
- Producción: HTTPS recomendado

**JWT (RFC 7519)**
- Estándar para tokens de autenticación
- Estructura: Header.Payload.Signature
- Firmado con clave secreta (HS256)

**REST (Representational State Transfer)**
- Estilo arquitectónico para APIs
- Endpoints por recurso
- Códigos de estado HTTP estándar

**JSON (JavaScript Object Notation)**
- Formato de intercambio de datos
- Content-Type: application/json
- Serialización/deserialización automática

**OAuth 2.0 (No implementado)**
- Estándar para autorización
- Posible futura implementación

### Infraestructura de Despliegue

**Desarrollo:**
- IIS Express (Visual Studio)
- Kestrel (dotnet run)
- Live Server (dashboard web)

**Producción (Recomendado):**
- IIS (Windows Server)
- Reverse proxy: Nginx o Apache
- Base de datos: SQL Server Standard/Enterprise
- Certificado SSL/TLS

**Requisitos de Sistema:**
- OS: Windows 10/11 o Windows Server 2019+
- CPU: 2+ cores
- RAM: 4 GB mínimo (8 GB recomendado)
- Storage: 500 MB para aplicación + espacio para BD

### Versionado y Build

**Versión Actual:** 1.0.0

**Compilación del Cliente:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:EnableCompressionInSingleFile=true
```

**Compilación de la API:**
```bash
dotnet publish -c Release -o ./publish
```

**Generación del Instalador:**
```bash
ISCC.exe UnilockerInstaller.iss
```

### Monitoreo y Logging

**Logging en API**
- ILogger<T> integrado en .NET
- Niveles: Information, Warning, Error
- Output: Console, File (configurable)

**Auditoría en Base de Datos**
- Tabla `AuditLogs`
- Registro automático de operaciones críticas
- Información: User, Action, Entity, Timestamp, IP

**Métricas (No implementadas)**
- Posibles mejoras futuras:
  - Application Insights
  - Prometheus + Grafana
  - ELK Stack (Elasticsearch, Logstash, Kibana)

### Testing (No implementado actualmente)

**Posibles Frameworks:**
- xUnit / NUnit para unit tests
- Moq para mocking
- Selenium para tests E2E del dashboard

---

*Última actualización: Diciembre 2025*  
*Versión del sistema: 1.0.0*

