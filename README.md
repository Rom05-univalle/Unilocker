# Unilocker - Sistema de Control de Acceso a Laboratorios

Sistema integral para la gestión y control de acceso a laboratorios de cómputo en instituciones educativas.

## Descripción

Unilocker es un sistema de tres componentes que permite:
- Control de acceso a computadoras en laboratorios mediante inicio de sesión
- Monitoreo en tiempo real de sesiones activas
- Gestión de reportes de problemas técnicos
- Auditoría completa de todas las acciones del sistema

## Arquitectura

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

## Estructura del Proyecto

```
UnilockerProyecto/
├── Unilocker.Api/          # API REST Backend (.NET 8)
├── Unilocker.Client/       # Cliente de escritorio WPF  
├── Unilocker.Web/          # Dashboard web para administración
├── Database/               # Scripts SQL de base de datos
├── installer/              # Instalador del cliente
└── README.md
```

## Componentes

### 1. Cliente WPF (Unilocker.Client)
Aplicación de escritorio que se ejecuta en cada computadora del laboratorio.

**Características:**
- Modo Kiosco: Bloquea el equipo hasta iniciar sesión
- Auto-inicio: Se ejecuta automáticamente al encender
- Registro de equipos: Configuración inicial con UUID único
- Reportes: Los usuarios pueden reportar problemas
- Heartbeat: Mantiene sesión activa con verificaciones cada 30 segundos

**Tecnologías:**
- .NET 8 WPF
- Material Design
- BCrypt para seguridad

[Ver documentación completa del Cliente](Unilocker.Client/README.md)

### 2. API REST (Unilocker.Api)
Backend que centraliza toda la lógica de negocio.

**Características:**
- Autenticación JWT con 2FA opcional
- CRUD completo para todas las entidades
- Control de sesiones activas
- Gestión de reportes de problemas
- Auditoría automática de todas las operaciones
- Endpoints RESTful documentados

**Tecnologías:**
- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt

[Ver documentación completa de la API](Unilocker.Api/README.md)

### 3. Dashboard Web (Unilocker.Web)
Interfaz administrativa para gestión del sistema.

**Características:**
- Dashboard visual con estadísticas en tiempo real
- Gestión de usuarios y roles
- Administración de infraestructura (Sucursales, Bloques, Aulas)
- Monitoreo de sesiones activas
- Gestión de reportes y problemas
- Visualización de auditoría
- Analíticas y reportes

**Tecnologías:**
- HTML5, CSS3, JavaScript (ES6+)
- Bootstrap 5
- Chart.js para gráficos
- Fetch API

[Ver documentación completa del Dashboard Web](Unilocker.Web/README.md)

## Base de Datos

### Entidades Principales
- **Roles**: Administrador, Docente, Estudiante
- **Users**: Usuarios del sistema con autenticación
- **Branches**: Sucursales/Sedes
- **Blocks**: Bloques/Edificios
- **Classrooms**: Aulas/Laboratorios
- **Computers**: Computadoras registradas
- **Sessions**: Sesiones activas e históricas
- **Reports**: Reportes de problemas
- **ProblemTypes**: Categorías de problemas
- **AuditLogs**: Registro de auditoría

Ver detalles en [Database/README.md](Database/README.md)

## Instalación Rápida

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

**Opción A: Usar instalador** (Recomendado)
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

## Instalador

El instalador automático incluye:
- Instalación del cliente en Program Files
- Configuración de URL de API durante instalación
- Auto-inicio de Windows (opcional)
- Acceso directo en escritorio
- Desinstalador completo

**Ubicación:** `installer/UnilockerClientSetup_v1.0.0.exe`

**Crear instalador:**
```bash
# Compilar cliente en modo Release
cd Unilocker.Client
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Usar Inno Setup con el script
cd installer
iscc UnilockerInstaller.iss
```

## Usuarios de Prueba

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| radmin | admin123 | Administrador |
| ruser | admin123 | Docente |
| estuser | admin123 | Estudiante |

## Configuración

### Cliente (appsettings.json)
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

### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVER;Database=UnilockerDBV1;..."
  },
  "Jwt": {
    "Key": "ClaveSecreta32CaracteresMinimo!!!",
    "Issuer": "UnilockerAPI",
    "Audience": "UnilockerClients",
    "ExpirationMinutes": 480
  }
}
```

### Web (js/api.js)
```javascript
const API_BASE_URL = 'http://localhost:5013/api';
```

## Seguridad

- **Autenticación JWT**: Tokens seguros con expiración configurable
- **Contraseñas hasheadas**: BCrypt con factor de trabajo 12
- **2FA opcional**: Autenticación de dos factores vía email
- **Auditoría completa**: Registro de todas las acciones
- **Modo Kiosco**: Previene bypass del sistema
- **UUIDs únicos**: Identificación única por equipo
- **Eliminación lógica**: Los datos no se eliminan físicamente

## Características Principales

### Modo Kiosco
- Bloquea el cierre de la aplicación hasta login exitoso
- Permite cierre con Alt+F4 solo si hay problemas de conexión
- Se minimiza después del login (no se puede cerrar)
- Pantalla completa sin bordes

### Control de Sesiones
- Inicio/fin automático de sesiones
- Heartbeat cada 30 segundos para mantener sesión activa
- Cierre automático de sesión al cerrar aplicación
- Historial completo de sesiones con duración

### Sistema de Reportes
- Reportar problemas desde el cliente
- Categorización por tipo de problema
- Estados: Pendiente, En Revisión, Resuelto
- Tracking completo con auditoría

### Dashboard Administrativo
- KPIs en tiempo real
- Filtros avanzados en todos los módulos
- Totalizadores dinámicos
- Exportación de datos
- Gráficos y analíticas

## Desarrollo

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

### Ejecutar en Desarrollo
```bash
# API
cd Unilocker.Api
dotnet watch run

# Cliente
cd Unilocker.Client
dotnet run

# Web
cd Unilocker.Web
# Usar Live Server en VS Code
```

## Tecnologías Utilizadas

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt.Net

### Frontend Cliente
- WPF (Windows Presentation Foundation)
- Material Design In XAML
- .NET 8

### Frontend Web
- HTML5, CSS3, JavaScript ES6+
- Bootstrap 5
- Chart.js
- Font Awesome

### Base de Datos
- SQL Server 2019+
- Stored Procedures
- Triggers
- Índices optimizados

## Licencia

Proyecto académico - Universidad del Valle

## Autor

Rodrigo Gutierrez Herrera (Rom05-univalle)

## Contacto

ghr0034560@est.univalle.edu

---

**Última actualización:** Diciembre 2025  
**Versión:** 1.0.0
