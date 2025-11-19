# 🔒 UNILOCKER - Sistema de Gestión de Laboratorios

Sistema completo de registro y gestión de computadoras para laboratorios universitarios.

## 📋 Descripción

Unilocker es un sistema que permite:
- Registro automático de computadoras en laboratorios
- Gestión de sesiones de uso
- Sistema de reportes de problemas
- Control de acceso por roles

## 🏗️ Arquitectura del Proyecto

```
UnilockerProyecto/
├── Unilocker.Api/          # Backend API REST (.NET 8)
│   ├── Controllers/        # Endpoints de la API
│   ├── Data/              # DbContext y configuración de BD
│   ├── DTOs/              # Data Transfer Objects
│   └── Models/            # Modelos de entidades
│
├── Unilocker.Client/       # Cliente Windows (WPF)
│   ├── Models/            # Modelos del cliente
│   ├── Services/          # Servicios (API, Hardware, Config)
│   └── Views/             # Ventanas de la aplicación
│
└── Database/              # Scripts SQL (opcional)
    └── schema.sql         # Script de creación de BD
```

## 🚀 Tecnologías Utilizadas

### Backend
- **Framework:** ASP.NET Core 8.0 Web API
- **ORM:** Entity Framework Core 8.0
- **Base de Datos:** SQL Server Express 2022
- **Documentación API:** Swagger/OpenAPI

### Cliente Windows
- **Framework:** WPF (.NET 8)
- **Detección Hardware:** System.Management
- **HTTP Client:** HttpClient + System.Net.Http.Json
- **Serialización:** Newtonsoft.Json

## ⚙️ Configuración e Instalación

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express 2022](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)
- [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup)
- Visual Studio 2022 o VS Code (opcional)

---

## 🗄️ 1. Configurar la Base de Datos

### Paso 1: Crear la base de datos

Ejecuta el script SQL en SSMS (ubicado en `/Database/schema.sql` o el que usaste en Sprint 1)

### Paso 2: Configurar acceso remoto

```sql
-- Crear login para la aplicación
CREATE LOGIN Unilocker_Access WITH PASSWORD = 'Uni2025!SecurePass';

-- Dar permisos
USE UnilockerDBV1;
CREATE USER Unilocker_Access FOR LOGIN Unilocker_Access;
ALTER ROLE db_datareader ADD MEMBER Unilocker_Access;
ALTER ROLE db_datawriter ADD MEMBER Unilocker_Access;
```

### Paso 3: Habilitar TCP/IP

1. Abrir **SQL Server Configuration Manager**
2. SQL Server Network Configuration → Protocols for SQLEXPRESS
3. Habilitar **TCP/IP**
4. Reiniciar servicio SQL Server

---

## 🌐 2. Configurar el Backend API

### Paso 1: Clonar el repositorio

```bash
git clone https://github.com/Rom05-univalle/unilocker.git
cd unilocker/Unilocker.Api
```

### Paso 2: Configurar connection string

Crea un archivo `appsettings.json` (o copia `appsettings.example.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_IP,1433;Database=UnilockerDBV1;User Id=Unilocker_Access;Password=Uni2025!SecurePass;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Paso 3: Restaurar paquetes y ejecutar

```bash
dotnet restore
dotnet build
dotnet run
```

La API estará disponible en: `http://localhost:5013/swagger`

---

## 💻 3. Configurar el Cliente Windows

### Paso 1: Ir a la carpeta del cliente

```bash
cd ../Unilocker.Client
```

### Paso 2: Configurar URL de la API

Edita `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:(puerto)"
  },
  "AppSettings": {
    "DataDirectory": "C:\\ProgramData\\Unilocker",
    "MachineIdFile": "machine.id",
    "RegisteredFlagFile": "registered.flag"
  }
}
```

### Paso 3: Compilar y ejecutar

```bash
dotnet restore
dotnet build
dotnet run
```

---

## 📦 Publicar el Cliente (Instalador)

Para crear un ejecutable portable:

```bash
cd Unilocker.Client
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El ejecutable estará en:
```
bin\Release\net8.0-windows\win-x64\publish\Unilocker.Client.exe
```

---

## 🧪 Pruebas

### Probar la API

```bash
# Health check
curl http://localhost:(puerto)/api/health

# Listar aulas
curl http://localhost:(puerto)/api/computers/classrooms
```

### Probar el Cliente

1. Ejecutar `Unilocker.Client.exe`
2. Seleccionar un aula
3. Registrar equipo
4. Verificar en la BD que se creó el registro

---

## 📊 Endpoints de la API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/health` | Health check de la API |
| GET | `/api/computers/classrooms` | Lista de aulas disponibles |
| POST | `/api/computers/register` | Registrar nueva computadora |
| GET | `/api/computers/{id}` | Obtener computadora por ID |

---

## 🔐 Seguridad

⚠️ **IMPORTANTE**: Este proyecto está en fase de desarrollo.

**Para producción, implementar:**
- [ ] Autenticación JWT
- [ ] HTTPS obligatorio
- [ ] Validación de inputs
- [ ] Rate limiting
- [ ] Logs de auditoría
- [ ] Cifrado de datos sensibles

---

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -am 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Crea un Pull Request

---

## 📝 Roadmap

### Sprint 1 ✅ (Completado)
- [x] Infraestructura y Base de Datos
- [x] Backend API REST básico
- [x] Cliente Windows de registro

### Sprint 2 🚧 (En progreso)
- [ ] Sistema de autenticación
- [ ] Gestión de sesiones
- [ ] Dashboard web para administradores

### Sprint 3 📅 (Planificado)
- [ ] Sistema de reportes de problemas
- [ ] Notificaciones en tiempo real
- [ ] Estadísticas y métricas

---

## 👥 Equipo de Desarrollo

- **Desarrollador Principal:** Rommel Rodirgo Gutierrez Herrera
- **Repositorio:** https://github.com/Rom05-univalle/Unilocker

---

## 📄 Licencia

Este proyecto es publico y de uso académico.

---

## 📞 Soporte

Para reportar bugs o solicitar features, crear un Issue en GitHub.
