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

**Nota:** Este es un proyecto académico desarrollado como parte del curso de Sistemas de Información.
