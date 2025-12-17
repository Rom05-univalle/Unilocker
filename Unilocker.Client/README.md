# Unilocker Client

Cliente de escritorio WPF para el sistema Unilocker de control de acceso a laboratorios.

## Descripción

Aplicación de escritorio que se ejecuta en cada computadora del laboratorio. Implementa un sistema de bloqueo (modo kiosco) que requiere inicio de sesión para desbloquear el equipo. Gestiona sesiones de usuarios y permite reportar problemas técnicos.

## Características

- Modo Kiosco: Bloquea el equipo hasta iniciar sesión válida
- Auto-inicio: Se ejecuta automáticamente al encender Windows
- Registro de equipos: Configuración inicial con UUID único por máquina
- Control de sesiones: Inicio/fin automático de sesiones
- Heartbeat: Mantiene sesión activa con verificaciones cada 30 segundos
- Sistema de reportes: Permite reportar problemas técnicos
- Interfaz moderna: Material Design con WPF
- Autenticación segura: BCrypt para contraseñas
- Offline tolerance: Manejo de pérdida de conexión

## Tecnologías

- .NET 8
- WPF (Windows Presentation Foundation)
- MaterialDesignInXAML para UI
- HttpClient para comunicación con API
- BCrypt.Net para seguridad
- System.Text.Json para serialización

## Estructura del Proyecto

```
Unilocker.Client/
├── App.xaml                    # Configuración de la aplicación
├── App.xaml.cs                 # Lógica de inicio
├── AssemblyInfo.cs             # Información del ensamblado
├── appsettings.json            # Configuración
├── MainWindow.xaml             # Ventana principal
├── MainWindow.xaml.cs          # Lógica ventana principal
├── Models/                     # Modelos de datos
│   ├── ComputerResponse.cs
│   └── RegisterComputerRequest.cs
├── Services/                   # Servicios de la aplicación
│   ├── ApiService.cs           # Comunicación con API
│   ├── ConfigService.cs        # Gestión de configuración
│   └── HardwareService.cs      # Información de hardware
├── Views/                      # Vistas adicionales
│   ├── RegisterWindow.xaml     # Ventana de registro
│   └── RegisterWindow.xaml.cs
└── Unilocker.Client.csproj     # Archivo de proyecto
```

## Instalación

### Opción 1: Usar Instalador (Recomendado)

1. Ejecutar `installer/UnilockerClientSetup_v1.0.0.exe`

2. Durante instalación:
   - Ingresar URL de la API (ej: `http://192.168.1.100:5013`)
   - Opcionalmente activar auto-inicio

3. El instalador:
   - Instala la aplicación en `C:\Program Files\Unilocker`
   - Crea acceso directo en escritorio
   - Configura auto-inicio (si se seleccionó)

### Opción 2: Compilar desde Código

```bash
cd Unilocker.Client
dotnet restore
dotnet build
dotnet run
```

### Opción 3: Compilar Ejecutable Standalone

```bash
cd Unilocker.Client
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El ejecutable estará en: `bin/Release/net8.0-windows/win-x64/publish/Unilocker.Client.exe`

## Configuración

### appsettings.json

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

**Parámetros:**
- `BaseUrl`: URL del servidor API (sin /api al final)
- `DataDirectory`: Carpeta donde se almacenan datos locales
- `MachineIdFile`: Archivo que contiene el UUID del equipo
- `RegisteredFlagFile`: Bandera que indica si el equipo está registrado

### Configuración de Heartbeat

Para cambiar el intervalo de heartbeat, editar `MainWindow.xaml.cs`:

```csharp
_heartbeatTimer = new DispatcherTimer
{
    Interval = TimeSpan.FromSeconds(30) // Cambiar aquí
};
```

### Auto-inicio en Windows

Para habilitar auto-inicio manualmente:

1. Presionar `Win + R`
2. Escribir `shell:startup`
3. Crear acceso directo a `Unilocker.Client.exe` en esa carpeta

O mediante registro:
```powershell
$exePath = "C:\Program Files\Unilocker\Unilocker.Client.exe"
$regPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $regPath -Name "Unilocker" -Value $exePath
```

## Flujo de Uso

### Primera Ejecución (Registro)

1. La aplicación detecta que no está registrada
2. Muestra ventana de registro
3. Usuario ingresa:
   - Nombre de la computadora
   - Selecciona Sucursal > Bloque > Aula
4. Se genera UUID único para la máquina
5. Se registra en la API mediante `POST /api/computers/register`
6. Se guarda información local (`machine.id` y `registered.flag`)

### Ejecuciones Posteriores

1. **Ventana de Login (Modo Kiosco)**
   - Pantalla completa sin bordes
   - No se puede cerrar con X (botón oculto)
   - Alt+F4 solo funciona si hay error de conexión
   - Usuario ingresa email y contraseña

2. **Autenticación**
   - Se envía `POST /api/auth/login`
   - Recibe token JWT
   - Inicia sesión mediante `POST /api/sessions/start`

3. **Ventana Principal (Post-Login)**
   - Muestra nombre del usuario
   - Muestra nombre de la computadora
   - Botón "Reportar Problema"
   - Botón "Cerrar Sesión"
   - Se minimiza automáticamente
   - **NO se puede cerrar** mientras haya sesión activa

4. **Heartbeat Automático**
   - Cada 30 segundos envía `PUT /api/sessions/{id}/heartbeat`
   - Mantiene sesión activa en servidor

5. **Cierre de Sesión**
   - Usuario hace clic en "Cerrar Sesión"
   - Se envía `PUT /api/sessions/{id}/end`
   - Vuelve a ventana de login

## Modo Kiosco

### Características de Seguridad

- **Pantalla completa**: Sin barra de título ni bordes
- **Topmost**: Siempre al frente
- **Bloqueo de cierre**: No se puede cerrar con X o Alt+F4 (excepto en errores)
- **Sin barra de tareas visible**: Aplicación cubre todo
- **Inicio automático**: Se ejecuta al encender Windows

### Desactivar Modo Kiosco (Desarrollo)

Editar `Views/LoginWindow.xaml`:

```xml
<!-- Cambiar de esto: -->
<Window WindowStyle="None" 
        AllowsTransparency="True" 
        Topmost="True"
        WindowState="Maximized">

<!-- A esto: -->
<Window WindowStyle="SingleBorderWindow" 
        WindowState="Normal"
        Width="800" Height="600">
```

## Sistema de Reportes

### Crear Reporte

1. Desde ventana principal, clic en "Reportar Problema"
2. Seleccionar tipo de problema:
   - Hardware
   - Software
   - Red
   - Periférico
   - Rendimiento
   - Otro
3. Ingresar descripción detallada
4. Enviar reporte
5. Se crea mediante `POST /api/reports`

### Tipos de Problemas

Los tipos se obtienen del endpoint `GET /api/problemtypes`.

## Gestión de Sesiones

### Inicio de Sesión

```csharp
var sessionRequest = new
{
    UserId = _currentUserId,
    ComputerId = _computerId
};

var response = await _apiService.PostAsync(
    "/api/sessions/start", 
    sessionRequest
);
```

### Heartbeat

```csharp
private async void HeartbeatTimer_Tick(object sender, EventArgs e)
{
    await _apiService.PutAsync(
        $"/api/sessions/{_currentSessionId}/heartbeat", 
        null
    );
}
```

### Fin de Sesión

```csharp
await _apiService.PutAsync(
    $"/api/sessions/{_currentSessionId}/end", 
    new { endMethod = "Normal" }
);
```

## Servicios

### ApiService

Gestiona toda la comunicación con el backend:

```csharp
public class ApiService
{
    public async Task<T> GetAsync<T>(string endpoint);
    public async Task<T> PostAsync<T>(string endpoint, object data);
    public async Task<T> PutAsync<T>(string endpoint, object data);
    public async Task<T> DeleteAsync<T>(string endpoint);
}
```

### ConfigService

Gestiona configuración local:

```csharp
public class ConfigService
{
    public string GetApiBaseUrl();
    public string GetDataDirectory();
    public string GetMachineId();
    public bool IsRegistered();
    public void SaveMachineId(string uuid);
    public void MarkAsRegistered();
}
```

### HardwareService

Obtiene información del hardware:

```csharp
public class HardwareService
{
    public string GetProcessorInfo();
    public string GetOperatingSystem();
    public string GetComputerName();
    public string GetMacAddress();
}
```

## Manejo de Errores

### Pérdida de Conexión

Si se pierde conexión con API:
- Heartbeat falla silenciosamente
- Usuario puede seguir trabajando
- Al cerrar sesión, se reintenta múltiples veces
- Si falla, se permite cerrar la aplicación

### Token Expirado

Si el token JWT expira:
- La aplicación detecta error 401
- Cierra sesión automáticamente
- Vuelve a login

### Computadora No Registrada

Si se detecta que no está registrada:
- Muestra ventana de registro
- No permite continuar sin registro

## Seguridad

### Almacenamiento de UUID

El UUID se almacena en:
- Archivo: `C:\ProgramData\Unilocker\machine.id`
- Generado con `Guid.NewGuid()`
- Se usa para identificar la computadora

### Autenticación

- Contraseñas nunca se almacenan localmente
- Token JWT se mantiene solo en memoria
- Se valida contra API en cada sesión

### Modo Kiosco

- Previene cierre accidental
- Fuerza autenticación
- Protege contra bypass

## Compilación

### Modo Debug

```bash
dotnet build -c Debug
```

### Modo Release

```bash
dotnet build -c Release
```

### Ejecutable Single-File

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Opciones:**
- `-r win-x64`: Runtime de Windows 64-bit
- `--self-contained true`: Incluye runtime de .NET
- `-p:PublishSingleFile=true`: Genera un solo archivo ejecutable

## Crear Instalador

1. Compilar aplicación en modo Release

2. Copiar ejecutable a carpeta `installer/output`

3. Ejecutar Inno Setup:
```bash
cd installer
iscc UnilockerInstaller.iss
```

4. El instalador se genera en `installer/Output/UnilockerClientSetup_v1.0.0.exe`

Ver más detalles en [installer/README.md](../installer/README.md)

## Dependencias (NuGet)

```xml
<ItemGroup>
  <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
  <PackageReference Include="MaterialDesignColors" Version="2.1.4" />
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
</ItemGroup>
```

## Personalización UI

### Colores Material Design

Editar `App.xaml`:

```xml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <!-- Tema Light/Dark -->
        <materialDesign:BundledTheme BaseTheme="Light" 
                                      PrimaryColor="Blue" 
                                      SecondaryColor="LightBlue" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

### Cambiar Logo

Reemplazar archivo de logo y actualizar en XAML:

```xml
<Image Source="/Resources/logo.png" Width="100" Height="100"/>
```

## Solución de Problemas

### No se conecta a la API
- Verificar que la API esté ejecutándose
- Confirmar URL correcta en `appsettings.json`
- Verificar firewall de Windows

### No se registra la computadora
- Verificar permisos en `C:\ProgramData\Unilocker`
- Confirmar conectividad con API
- Revisar logs de la API

### Heartbeat falla
- Verificar conectividad de red
- Confirmar que sesión no haya expirado
- Revisar logs del servidor

### No inicia automáticamente
- Verificar registro de Windows
- Revisar carpeta de inicio: `shell:startup`
- Confirmar permisos de ejecución

### Aplicación se cierra inesperadamente
- Revisar logs de Windows Event Viewer
- Verificar versión de .NET 8 instalada
- Confirmar dependencias instaladas

## Logs y Debugging

### Habilitar Logs Detallados

Agregar en `App.xaml.cs`:

```csharp
private void ConfigureLogging()
{
    Debug.WriteLine("Iniciando Unilocker Client...");
    // Agregar logging personalizado
}
```

### Ver Logs en Visual Studio

- Presionar F5 para ejecutar en modo debug
- Ver Output window (Ctrl+Alt+O)
- Logs aparecen en tiempo real

## Requisitos del Sistema

### Mínimos
- Windows 10 64-bit
- .NET 8 Runtime
- 2 GB RAM
- 100 MB espacio en disco

### Recomendados
- Windows 11 64-bit
- 4 GB RAM
- Conexión a Internet estable

## Desinstalación

### Usando el Desinstalador

1. Panel de Control > Programas > Desinstalar un programa
2. Seleccionar "Unilocker Client"
3. Clic en "Desinstalar"

### Manualmente

```powershell
# Eliminar archivos
Remove-Item "C:\Program Files\Unilocker" -Recurse -Force

# Eliminar datos
Remove-Item "C:\ProgramData\Unilocker" -Recurse -Force

# Eliminar auto-inicio
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Unilocker"

# Eliminar acceso directo
Remove-Item "$env:USERPROFILE\Desktop\Unilocker.lnk"
```

## Mejoras Futuras

- Reconexión automática tras pérdida de red
- Caché local de credenciales para modo offline
- Actualización automática del cliente
- Notificaciones push desde servidor
- Captura de screenshots para reportes
- Modo de mantenimiento remoto

## Contacto

Para soporte o consultas:
- Email: ghr0034560@est.univalle.edu
- Autor: Rodrigo Gutierrez Herrera

---

**Versión:** 1.0.0  
**Última actualización:** Diciembre 2025
