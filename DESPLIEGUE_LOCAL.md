# 🚀 GUÍA DE DESPLIEGUE LOCAL - UNILOCKER

Esta guía te ayudará a configurar el entorno completo de Unilocker en un nuevo equipo, desde cero hasta tener el sistema completamente funcional.

---

## 📋 Tabla de Contenidos

1. [Prerrequisitos](#-1-prerrequisitos)
2. [Configurar SQL Server](#-2-configurar-sql-server)
3. [Restaurar Base de Datos](#-3-restaurar-base-de-datos)
4. [Configurar la API](#-4-configurar-la-api-backend)
5. [Configurar el Cliente Desktop](#-5-configurar-el-cliente-desktop-wpf)
6. [Configurar la Web V2](#-6-configurar-la-web-v2)
7. [Ejecutar el Sistema](#-7-ejecutar-el-sistema)
8. [Verificación y Pruebas](#-8-verificación-y-pruebas)
9. [Solución de Problemas](#-9-solución-de-problemas)

---

## 📦 1. Prerrequisitos

### Software Requerido

| Software | Versión | Link de Descarga |
|----------|---------|------------------|
| .NET SDK | 8.0 o superior | [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0) |
| SQL Server Express | 2022 | [Descargar](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) |
| SQL Server Management Studio (SSMS) | Última | [Descargar](https://aka.ms/ssmsfullsetup) |
| Visual Studio Code | Última | [Descargar](https://code.visualstudio.com/) |
| Git | Última | [Descargar](https://git-scm.com/downloads) |

### Extensiones Recomendadas para VS Code

- C# Dev Kit
- Live Server (para la web)
- SQL Server (mssql)

---

## 🗄️ 2. Configurar SQL Server

### Paso 1: Instalar SQL Server Express

1. Ejecutar el instalador de SQL Server Express 2022
2. Seleccionar instalación **Básica** o **Personalizada**
3. Anotar el nombre de la instancia (por defecto: `SQLEXPRESS`)
4. Anotar el nombre del servidor (ejemplo: `DESKTOP-C82PFDH\SQLEXPRESS`)

### Paso 2: Habilitar TCP/IP y Puerto 1433

1. Abrir **SQL Server Configuration Manager**
2. Ir a: `SQL Server Network Configuration` → `Protocols for SQLEXPRESS`
3. Click derecho en **TCP/IP** → `Enable`
4. Click derecho en **TCP/IP** → `Properties` → pestaña `IP Addresses`
5. Ir a la sección **IPALL**:
   - `TCP Port`: **1433**
   - `TCP Dynamic Ports`: **dejar vacío**
6. Click en **OK** y reiniciar el servicio SQL Server

### Paso 3: Habilitar Autenticación Mixta

1. Abrir **SQL Server Management Studio (SSMS)**
2. Conectarse con autenticación de Windows
3. Click derecho en el servidor → `Properties`
4. Ir a `Security`
5. Seleccionar: **SQL Server and Windows Authentication mode**
6. Click en **OK**
7. Reiniciar el servicio SQL Server

### Paso 4: Crear Login para la Aplicación

Ejecutar en SSMS (Nueva Consulta):

```sql
-- Crear login con contraseña
CREATE LOGIN Unilocker_Access WITH PASSWORD = 'Uni2025!SecurePass';
GO

-- Verificar que se creó
SELECT name, type_desc, create_date 
FROM sys.server_principals 
WHERE name = 'Unilocker_Access';
```

---

## 💾 3. Restaurar Base de Datos

### Opción A: Restaurar desde .BAK

1. Copiar tu archivo `.bak` a una ubicación accesible (ej: `C:\Backups\UnilockerDBV1.bak`)

2. Abrir SSMS y ejecutar:

```sql
-- Verificar el contenido del backup
RESTORE FILELISTONLY 
FROM DISK = 'C:\Backups\UnilockerDBV1.bak';
GO

-- Restaurar la base de datos
RESTORE DATABASE UnilockerDBV1
FROM DISK = 'C:\Backups\UnilockerDBV1.bak'
WITH 
    MOVE 'UnilockerDBV1' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\UnilockerDBV1.mdf',
    MOVE 'UnilockerDBV1_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\UnilockerDBV1_log.ldf',
    REPLACE;
GO
```

> **Nota**: Ajusta las rutas `MOVE` según tu instalación de SQL Server. Puedes encontrar la ruta correcta ejecutando:
> ```sql
> SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('master');
> ```

### Opción B: Restaurar desde GUI de SSMS

1. En SSMS, click derecho en `Databases` → `Restore Database...`
2. Seleccionar `Device` y buscar tu archivo `.bak`
3. Marcar `Overwrite the existing database (WITH REPLACE)`
4. Ir a pestaña `Files` y verificar las rutas de destino
5. Click en **OK**

### Paso 3: Asignar Permisos al Usuario

```sql
USE UnilockerDBV1;
GO

-- Crear usuario en la base de datos
CREATE USER Unilocker_Access FOR LOGIN Unilocker_Access;
GO

-- Asignar roles
ALTER ROLE db_datareader ADD MEMBER Unilocker_Access;
ALTER ROLE db_datawriter ADD MEMBER Unilocker_Access;
GO

-- Dar permisos de ejecución (para stored procedures si los hay)
GRANT EXECUTE TO Unilocker_Access;
GO
```

### Paso 4: Verificar la Conexión

```sql
-- Probar que puedes conectarte con el nuevo usuario
-- Cierra SSMS y vuelve a abrir
-- Al conectar, usa:
--   Authentication: SQL Server Authentication
--   Login: Unilocker_Access
--   Password: Uni2025!SecurePass

-- Si la conexión funciona, ejecuta:
SELECT COUNT(*) as TotalTablas FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

---

## ⚙️ 4. Configurar la API (Backend)

### Paso 1: Clonar o Copiar el Proyecto

```bash
# Si usas Git
git clone https://github.com/Rom05-univalle/Unilocker.git
cd Unilocker/UnilockerProyecto/Unilocker.Api

# O simplemente copia la carpeta del proyecto
```

### Paso 2: Configurar Connection String

1. Abrir: `Unilocker.Api/appsettings.json`

2. **Obtener tu nombre de servidor SQL**:
   - Abrir SSMS
   - El nombre del servidor aparece al conectarte (ejemplo: `DESKTOP-C82PFDH\SQLEXPRESS`)

3. Editar el `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR\\SQLEXPRESS,1433;Database=UnilockerDBV1;User Id=Unilocker_Access;Password=Uni2025!SecurePass;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "UnilockerSecretKey2025!MuySegura32CaracteresMinimo",
    "Issuer": "UnilockerAPI",
    "Audience": "UnilockerClients",
    "ExpirationMinutes": 480
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tucorreo@gmail.com",
    "SenderName": "Unilocker System",
    "Password": "tu_app_password_de_gmail"
  }
}
```

**Reemplazar:**
- `TU_SERVIDOR` → Nombre de tu servidor SQL (ejemplo: `DESKTOP-C82PFDH`)
- `tucorreo@gmail.com` → Tu correo de Gmail (para 2FA)
- `tu_app_password_de_gmail` → [Generar App Password](https://support.google.com/accounts/answer/185833)

### Paso 3: Verificar Puerto de la API

1. Abrir: `Unilocker.Api/Properties/launchSettings.json`

2. Verificar o cambiar el puerto:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://0.0.0.0:5013",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Puerto por defecto**: `5013`

### Paso 4: Restaurar Paquetes NuGet

```bash
cd Unilocker.Api
dotnet restore
```

### Paso 5: Compilar la API

```bash
dotnet build
```

Si hay errores, revisar:
- Connection string correcta
- SQL Server corriendo
- Permisos del usuario

---

## 🖥️ 5. Configurar el Cliente Desktop (WPF)

### Paso 1: Ir a la Carpeta del Cliente

```bash
cd ../Unilocker.Client
```

### Paso 2: Configurar URL de la API

1. Abrir: `Unilocker.Client/appsettings.json`

2. Editar:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5013"
  },
  "AppSettings": {
    "DataDirectory": "C:\\ProgramData\\Unilocker",
    "MachineIdFile": "machine.id",
    "RegisteredFlagFile": "registered.flag"
  }
}
```

**Cambiar:**
- Si la API está en otro puerto, cambiar `5013`
- Si la API está en otro equipo, cambiar `localhost` por la IP (ejemplo: `http://192.168.0.5:5013`)

### Paso 3: Compilar el Cliente

```bash
dotnet restore
dotnet build
```

---

## 🌐 6. Configurar la Web V2

### Paso 1: Configurar URL de la API

Hay **DOS archivos** que debes editar con la URL de tu API:

#### Archivo 1: `Unilocker.Web/js/api.js`

```javascript
// Línea 4
export const API_BASE_URL = "http://localhost:5013";
```

#### Archivo 2: `Unilocker.Web/js/auth.js`

```javascript
// Línea 2
const API_BASE_URL = "http://localhost:5013";
```

**Cambiar:**
- `localhost` por la **IP de tu equipo** donde corre la API si quieres acceder desde otros dispositivos
- Ejemplo para red local: `http://192.168.0.5:5013`

### Paso 2: Obtener tu IP Local (Opcional)

Si quieres acceder desde otros dispositivos en la red:

**Windows (PowerShell):**
```powershell
ipconfig | findstr IPv4
```

**Resultado ejemplo:**
```
IPv4 Address. . . . . . . . . . . : 192.168.0.5
```

Usa esa IP en los archivos de configuración.

---

## ▶️ 7. Ejecutar el Sistema

### Paso 1: Iniciar SQL Server

Verificar que el servicio está corriendo:

```powershell
# PowerShell (como administrador)
Get-Service -Name MSSQL*

# Iniciar si está detenido
Start-Service -Name "MSSQL`$SQLEXPRESS"
```

### Paso 2: Iniciar la API

**Opción A: Desde Visual Studio Code**

1. Abrir la carpeta `Unilocker.Api` en VS Code
2. Presionar `F5` (o ir a Run → Start Debugging)
3. La API se ejecutará en `http://localhost:5013`
4. Se abrirá Swagger en el navegador: `http://localhost:5013/swagger`

**Opción B: Desde Terminal**

```bash
cd Unilocker.Api
dotnet run
```

**Verificar que funciona:**
- Navegar a: `http://localhost:5013/swagger`
- Deberías ver la interfaz de Swagger con todos los endpoints

### Paso 3: Iniciar la Web

**Opción A: Con Live Server (VS Code)**

1. Instalar extensión "Live Server" en VS Code
2. Abrir la carpeta `Unilocker.Web`
3. Click derecho en `login.html` → `Open with Live Server`
4. Se abrirá en el navegador (usualmente en `http://127.0.0.1:5500`)

**Opción B: Directamente con el Navegador**

1. Navegar a la carpeta `Unilocker.Web`
2. Hacer doble click en `login.html`
3. Se abrirá en tu navegador predeterminado

> **⚠️ Importante**: Por temas de CORS y módulos ES6, se recomienda usar Live Server.

### Paso 4: Iniciar el Cliente Desktop (Opcional)

**Desde Visual Studio:**

1. Abrir `Unilocker.Client.sln` en Visual Studio
2. Presionar `F5` o click en el botón ▶️ Start
3. Se abrirá la aplicación WPF

**Desde Terminal:**

```bash
cd Unilocker.Client
dotnet run
```

---

## ✅ 8. Verificación y Pruebas

### Test 1: Health Check de la API

```bash
# PowerShell
Invoke-WebRequest -Uri "http://localhost:5013/api/health"

# O en el navegador:
http://localhost:5013/api/health
```

**Respuesta esperada:**
```json
{
  "status": "healthy",
  "connected": true,
  "timestamp": "2025-12-06T20:00:00Z",
  "database": "UnilockerDBV1"
}
```

### Test 2: Login en la Web

1. Abrir `http://127.0.0.1:5500/login.html` (o tu URL de Live Server)
2. Credenciales de prueba:
   - **Usuario**: `radmin`
   - **Contraseña**: `123456` (o la que configuraste)
3. Si el login funciona, deberías ver el dashboard

### Test 3: Verificar Conexión a Base de Datos

En SSMS, ejecutar:

```sql
-- Ver última actividad
SELECT TOP 10 * FROM AuditLog ORDER BY ActionDate DESC;

-- Ver usuarios registrados
SELECT Id, Username, Email, FirstName, LastName FROM [User];

-- Ver computadoras registradas
SELECT TOP 10 c.*, cl.Name as ClassroomName 
FROM Computer c
INNER JOIN Classroom cl ON c.ClassroomId = cl.Id
ORDER BY c.CreatedAt DESC;
```

---

## 🛠️ 9. Solución de Problemas

### Problema: "Cannot connect to SQL Server"

**Síntomas:**
- La API no inicia
- Error: `A network-related or instance-specific error occurred`

**Soluciones:**

1. **Verificar que SQL Server esté corriendo:**
   ```powershell
   Get-Service MSSQL*
   ```

2. **Verificar Connection String:**
   - Formato correcto: `Server=SERVIDOR\\SQLEXPRESS,1433;...`
   - Verificar nombre del servidor en SSMS

3. **Verificar TCP/IP habilitado:**
   - SQL Server Configuration Manager
   - Protocols for SQLEXPRESS → TCP/IP = Enabled

4. **Verificar puerto 1433:**
   ```powershell
   Test-NetConnection -ComputerName localhost -Port 1433
   ```

5. **Verificar login existe:**
   ```sql
   SELECT * FROM sys.server_principals WHERE name = 'Unilocker_Access';
   ```

---

### Problema: "CORS policy: No 'Access-Control-Allow-Origin' header"

**Síntomas:**
- En la consola del navegador aparece error de CORS
- La web no puede conectarse a la API

**Soluciones:**

1. **Usar Live Server** en lugar de abrir el HTML directamente

2. **Verificar configuración CORS en la API:**
   - Archivo: `Unilocker.Api/Program.cs`
   - Debe existir:
   ```csharp
   builder.Services.AddCors(options => {
       options.AddPolicy("AllowAll", policy => {
           policy.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   
   // Y más abajo:
   app.UseCors("AllowAll");
   ```

---

### Problema: "API devuelve 401 Unauthorized"

**Síntomas:**
- Login falla
- Otros endpoints devuelven 401

**Soluciones:**

1. **Verificar que el usuario existe en la BD:**
   ```sql
   SELECT * FROM [User] WHERE Username = 'radmin';
   ```

2. **Verificar contraseña BCrypt:**
   - La contraseña debe estar hasheada con BCrypt
   - Debe tener al menos 6 caracteres antes de hashear

3. **Verificar rol Administrador:**
   ```sql
   SELECT u.Username, r.Name as RoleName
   FROM [User] u
   INNER JOIN Role r ON u.RoleId = r.Id
   WHERE u.Username = 'radmin';
   ```
   - Debe devolver: RoleName = "Administrador"

---

### Problema: "Cannot read properties of undefined"

**Síntomas:**
- JavaScript errors en la consola del navegador
- La web no carga correctamente

**Soluciones:**

1. **Verificar que la API esté corriendo:**
   - Ir a `http://localhost:5013/swagger`

2. **Verificar configuración de URL en la web:**
   - Revisar `api.js` y `auth.js`
   - URL debe coincidir con donde corre la API

3. **Limpiar caché del navegador:**
   - Ctrl + Shift + R (hard reload)
   - O abrir en ventana de incógnito

---

### Problema: "Port 5013 already in use"

**Síntomas:**
- La API no inicia
- Error: `Failed to bind to address http://0.0.0.0:5013`

**Soluciones:**

1. **Cerrar procesos que usen el puerto:**
   ```powershell
   # Ver qué proceso usa el puerto
   netstat -ano | findstr :5013
   
   # Matar el proceso (usa el PID del comando anterior)
   taskkill /PID <PID> /F
   ```

2. **Cambiar el puerto:**
   - Editar `Unilocker.Api/Properties/launchSettings.json`
   - Cambiar `5013` por otro puerto (ej: `5014`)
   - Actualizar las URLs en la web

---

## 📝 Checklist de Configuración

Usa esta lista para verificar que todo está configurado:

### SQL Server
- [ ] SQL Server instalado y corriendo
- [ ] TCP/IP habilitado en puerto 1433
- [ ] Autenticación mixta habilitada
- [ ] Login `Unilocker_Access` creado
- [ ] Base de datos restaurada desde .BAK
- [ ] Usuario tiene permisos en la base de datos

### API
- [ ] .NET 8 SDK instalado
- [ ] `appsettings.json` configurado con Connection String correcta
- [ ] Paquetes NuGet restaurados (`dotnet restore`)
- [ ] API compila sin errores (`dotnet build`)
- [ ] API corre y responde en `http://localhost:5013/swagger`

### Web
- [ ] `js/api.js` configurado con URL correcta de la API
- [ ] `js/auth.js` configurado con URL correcta de la API
- [ ] Live Server instalado en VS Code (opcional pero recomendado)
- [ ] Web abre correctamente en el navegador

### Cliente Desktop (Opcional)
- [ ] `appsettings.json` configurado con URL de la API
- [ ] Cliente compila sin errores
- [ ] Cliente puede conectarse a la API

---

## 🔄 Cambiar a Otro Equipo

Si quieres mover el sistema a otro equipo:

### En el Nuevo Equipo:

1. **Instalar prerrequisitos** (ver sección 1)
2. **Copiar el backup** `.bak` de la base de datos
3. **Seguir pasos 2-7** de esta guía
4. **Actualizar IPs** si es necesario:
   - En la API no hay que cambiar nada (escucha en `0.0.0.0`)
   - En la web, cambiar `localhost` por la IP del nuevo equipo en:
     - `js/api.js`
     - `js/auth.js`
   - En el cliente desktop, cambiar en `appsettings.json`

### Para Acceso en Red Local:

Si quieres que otros equipos accedan:

1. **En el equipo donde corre la API:**
   - Obtener IP: `ipconfig`
   - Abrir firewall para el puerto 5013:
   ```powershell
   # PowerShell como administrador
   New-NetFirewallRule -DisplayName "Unilocker API" -Direction Inbound -Protocol TCP -LocalPort 5013 -Action Allow
   ```

2. **En otros equipos:**
   - Cambiar `localhost` por la IP del servidor en archivos de configuración

---

## 📞 Soporte

Si tienes problemas que no están en esta guía:

1. Revisa los logs de la API (aparecen en la terminal donde corre `dotnet run`)
2. Revisa la consola del navegador (F12) para errores JavaScript
3. Verifica conexión a SQL Server con SSMS
4. Crea un issue en GitHub con detalles del error

---

## 📚 Recursos Adicionales

- [Documentación .NET 8](https://learn.microsoft.com/es-es/dotnet/core/whats-new/dotnet-8)
- [SQL Server Configuration Manager](https://learn.microsoft.com/es-es/sql/relational-databases/sql-server-configuration-manager)
- [BCrypt Password Hashing](https://github.com/BcryptNet/bcrypt.net)
- [JWT Authentication](https://jwt.io/)

---

**Última actualización:** Diciembre 2025  
**Versión del documento:** 1.0  
**Autor:** Rommel Rodrigo Gutierrez Herrera
