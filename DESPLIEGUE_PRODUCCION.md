# 📦 GUÍA DE DESPLIEGUE EN PRODUCCIÓN - UNILOCKER CLIENT

Esta guía explica cómo compilar, empaquetar y distribuir el cliente de escritorio de Unilocker para instalación en producción.

---

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#-1-requisitos-previos)
2. [Compilar para Producción](#-2-compilar-para-producción)
3. [Crear Instalador](#-3-crear-instalador)
4. [Distribución](#-4-distribución)
5. [Instalación en Cliente](#-5-instalación-en-cliente)
6. [Configuración Post-Instalación](#-6-configuración-post-instalación)
7. [Actualizar Versiones](#-7-actualizar-versiones)
8. [Solución de Problemas](#-8-solución-de-problemas)

---

## ✅ 1. Requisitos Previos

### En tu Máquina de Desarrollo

| Software | Versión | Link |
|----------|---------|------|
| .NET SDK | 8.0 | [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Visual Studio 2022 (Opcional) | Community+ | [Descargar](https://visualstudio.microsoft.com/) |
| Inno Setup (para instalador) | 6.x | [Descargar](https://jrsoftware.org/isdl.php) |

---

## 🔨 2. Compilar para Producción

### Opción A: Publicación con dotnet CLI (Recomendado)

#### Paso 1: Abrir PowerShell en la carpeta del proyecto

```powershell
cd "c:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Unilocker.Client"
```

#### Paso 2: Limpiar compilaciones anteriores

```powershell
dotnet clean
Remove-Item -Recurse -Force .\bin\Release -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\publish -ErrorAction SilentlyContinue
```

#### Paso 3: Publicar la aplicación

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\win-x64
```

**Explicación de parámetros:**
- `-c Release`: Compilación en modo Release (optimizado)
- `-r win-x64`: Runtime para Windows 64-bit
- `--self-contained true`: Incluye el runtime de .NET (no requiere instalación de .NET en el cliente)
- `-p:PublishSingleFile=true`: Genera un único archivo ejecutable
- `-p:IncludeNativeLibrariesForSelfExtract=true`: Incluye librerías nativas
- `-o .\publish\win-x64`: Directorio de salida

#### Paso 4: Verificar la publicación

```powershell
Get-ChildItem .\publish\win-x64
```

**Deberías ver:**
- `Unilocker.Client.exe` (el ejecutable principal, ~100-150 MB aprox.)
- `appsettings.json` (archivo de configuración)

### Opción B: Publicación desde Visual Studio

1. Abrir solución en Visual Studio
2. Click derecho en proyecto `Unilocker.Client` → `Publish...`
3. Configurar:
   - Target: Folder
   - Configuration: Release
   - Target Runtime: win-x64
   - Deployment Mode: Self-contained
4. Click en **Publish**

---

## 📦 3. Crear Instalador

### Opción A: Instalador con Inno Setup (Recomendado)

#### Paso 1: Crear script de Inno Setup

Crear archivo `UnilockerInstaller.iss` en la carpeta raíz del proyecto:

```iss
; Script de instalación Unilocker Client
; Generado para Inno Setup 6

#define MyAppName "Unilocker Client"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Universidad Privada del Valle"
#define MyAppURL "https://www.univalle.edu"
#define MyAppExeName "Unilocker.Client.exe"

[Setup]
; Información de la aplicación
AppId={{YOUR-UNIQUE-GUID-HERE}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Unilocker
DisableProgramGroupPage=yes
LicenseFile=LICENSE.txt
OutputDir=installer
OutputBaseFilename=UnilockerClientSetup_v{#MyAppVersion}
SetupIconFile=icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "startupicon"; Description: "Ejecutar al iniciar Windows (Recomendado para laboratorios)"; GroupDescription: "Opciones de inicio:"; Flags: unchecked

[Files]
Source: "Unilocker.Client\publish\win-x64\Unilocker.Client.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Unilocker.Client\publish\win-x64\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion onlyifdoesntexist
; Nota: onlyifdoesntexist preserva configuraciones existentes en actualizaciones

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Ejecutar después de la instalación
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Limpiar archivos de configuración al desinstalar
Type: filesandordirs; Name: "{commonappdata}\Unilocker"
Type: files; Name: "{app}\appsettings.json"

[Registry]
; Remover del inicio automático al desinstalar
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "UnilockerClient"; Flags: deletevalue uninsdeletevalue; Tasks: startupicon

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Crear directorio de datos
    if not DirExists(ExpandConstant('{commonappdata}\Unilocker')) then
      CreateDir(ExpandConstant('{commonappdata}\Unilocker'));
  end;
end;
```

> **Nota**: Reemplaza `YOUR-UNIQUE-GUID-HERE` con un GUID único. Puedes generarlo en PowerShell:
> ```powershell
> [guid]::NewGuid().ToString()
> ```

#### Paso 2: Compilar el instalador

```powershell
# Navegar a la carpeta del proyecto
cd "c:\Proyecto de sistemas-Unilocker\UnilockerProyecto"

# Compilar con Inno Setup (ajustar ruta si es necesario)
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" UnilockerInstaller.iss
```

**Resultado**: `installer\UnilockerClientSetup_v1.0.0.exe` (archivo instalador)

### Opción B: Instalador Portable (Sin Instalador)

Si prefieres distribución sin instalador:

```powershell
# Crear carpeta de distribución
New-Item -ItemType Directory -Path ".\portable" -Force

# Copiar archivos publicados
Copy-Item ".\Unilocker.Client\publish\win-x64\*" -Destination ".\portable\" -Recurse -Force

# Crear archivo README
@"
UNILOCKER CLIENT - VERSIÓN PORTABLE
=====================================

INSTALACIÓN:
1. Copia esta carpeta a C:\Program Files\Unilocker (o donde desees)
2. Ejecuta Unilocker.Client.exe
3. Sigue el asistente de configuración

REQUISITOS:
- Windows 10/11 64-bit
- Conexión a red (para comunicarse con la API)

NOTAS:
- Esta versión NO requiere instalación
- Los datos se guardan en C:\ProgramData\Unilocker
- Para desinstalar, simplemente elimina la carpeta

SOPORTE:
Universidad Privada del Valle
"@ | Out-File -FilePath ".\portable\README.txt" -Encoding UTF8

# Comprimir en ZIP
Compress-Archive -Path ".\portable\*" -DestinationPath ".\UnilockerClient_Portable_v1.0.0.zip" -Force
```

---

## 📤 4. Distribución

### Métodos de Distribución

#### Opción 1: Servidor Web Interno

```powershell
# Copiar instalador a un servidor web
Copy-Item ".\installer\UnilockerClientSetup_v1.0.0.exe" -Destination "\\servidor\compartido\Software\Unilocker\"
```

Luego los usuarios pueden descargarlo desde una URL interna.

#### Opción 2: Compartir por Red

```powershell
# Compartir carpeta
New-SmbShare -Name "UnilockerInstall" -Path ".\installer" -ReadAccess "Everyone"
```

#### Opción 3: USB/Medios Físicos

Simplemente copia el instalador a USB y distribúyelo físicamente.

#### Opción 4: Implementación con GPO (Group Policy)

Para entornos empresariales con Active Directory:

1. Copiar el `.msi` (si usas WiX) al SYSVOL
2. Crear GPO de distribución de software
3. Asignar a las OUs correspondientes

---

## 💻 5. Instalación en Cliente

### Proceso de Instalación para el Usuario Final

#### Paso 1: Ejecutar el Instalador

1. Hacer doble click en `UnilockerClientSetup_v1.0.0.exe`
2. Si aparece UAC (Control de Cuentas), click en **Sí**

#### Paso 2: Asistente de Instalación

1. **Bienvenida**: Click en `Siguiente`
2. **Licencia**: Leer y aceptar → `Siguiente`
3. **Ubicación**: Dejar por defecto `C:\Program Files\Unilocker` → `Siguiente`
4. **Opciones**:
   - ✅ Crear icono en escritorio (opcional)
   - ✅ **Ejecutar al iniciar Windows** (RECOMENDADO para laboratorios)
5. **Instalar**: Click en `Instalar`
6. **Finalizar**: Marcar "Ejecutar Unilocker Client" → `Finalizar`

#### Paso 3: Primera Ejecución

Al ejecutar por primera vez, aparecerá:

1. **Ventana de Configuración Inicial**:
   - Ingresar URL de la API (ej: `http://192.168.0.5:5013`)
   - Click en **Probar Conexión**
   - Si es exitoso, click en **Guardar y Continuar**

2. **Ventana de Registro del Equipo**:
   - El sistema detecta automáticamente el hardware
   - Ingresar nombre del equipo (ej: "LAB-PC-01")
   - Seleccionar aula/laboratorio
   - Click en **Registrar Equipo**

3. **Reiniciar la Aplicación**:
   - La app se cierra automáticamente
   - Volver a ejecutar `Unilocker.Client.exe`

4. **Login**:
   - Ingresar usuario y contraseña
   - Sistema en modo kiosco activado ✅

---

## ⚙️ 6. Configuración Post-Instalación

### Configurar URL de la API Manualmente

Si necesitas cambiar la URL después de la instalación:

**Archivo**: `C:\Program Files\Unilocker\appsettings.json`

```json
{
  "ApiSettings": {
    "BaseUrl": "http://TU_SERVIDOR:5013"
  },
  "AppSettings": {
    "DataDirectory": "C:\\ProgramData\\Unilocker",
    "MachineIdFile": "machine.id",
    "RegisteredFlagFile": "registered.flag"
  }
}
```

### Verificar Inicio Automático

**Método 1: Registro de Windows**

```powershell
Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "UnilockerClient" -ErrorAction SilentlyContinue
```

**Método 2: Task Manager**

1. `Ctrl + Shift + Esc`
2. Pestaña **Startup**
3. Buscar **UnilockerClient**

### Desregistrar un Equipo

Si necesitas desregistrar un equipo (para volver a configurarlo):

1. Ejecutar la app como **Administrador**
2. Iniciar sesión con usuario admin
3. Click en botón **Desregistrar Equipo** (solo visible para admins)
4. Confirmar la acción

O manualmente:

```powershell
# Eliminar archivos de configuración
Remove-Item -Path "C:\ProgramData\Unilocker\*" -Force
```

---

## 🔄 7. Actualizar Versiones

### Actualización Simple (Sobrescribir)

1. Compilar nueva versión (seguir paso 2)
2. Crear nuevo instalador con versión actualizada
3. Ejecutar instalador en equipos existentes
4. El instalador **preserva** el archivo `appsettings.json` (no lo sobrescribe)

### Actualización Automática (Futuro)

Para implementar actualizaciones automáticas:

1. Implementar servicio de verificación de versiones en la API
2. Cliente verifica al iniciar si hay nueva versión disponible
3. Descarga e instala automáticamente (requiere permisos elevados)

---

## 🛠️ 8. Solución de Problemas

### Problema: "No se puede conectar a la API"

**Síntomas:**
- Ventana de configuración muestra error de conexión
- Cliente no puede comunicarse con el servidor

**Soluciones:**

1. **Verificar que la API esté corriendo:**
   ```powershell
   Test-NetConnection -ComputerName TU_SERVIDOR -Port 5013
   ```

2. **Verificar firewall:**
   ```powershell
   # En el servidor donde corre la API
   New-NetFirewallRule -DisplayName "Unilocker API" -Direction Inbound -Protocol TCP -LocalPort 5013 -Action Allow
   ```

3. **Verificar URL en appsettings.json:**
   - Debe ser la IP del servidor, no `localhost` (excepto si API está en el mismo equipo)

---

### Problema: "La aplicación no se inicia automáticamente"

**Síntomas:**
- Al iniciar Windows, Unilocker no aparece

**Soluciones:**

1. **Verificar entrada en el registro:**
   ```powershell
   Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "UnilockerClient"
   ```

2. **Agregar manualmente:**
   ```powershell
   $exePath = "C:\Program Files\Unilocker\Unilocker.Client.exe"
   Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "UnilockerClient" -Value "`"$exePath`""
   ```

3. **Crear tarea programada:**
   ```powershell
   $action = New-ScheduledTaskAction -Execute "C:\Program Files\Unilocker\Unilocker.Client.exe"
   $trigger = New-ScheduledTaskTrigger -AtLogOn
   Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "Unilocker Client" -Description "Cliente de control de laboratorios"
   ```

---

### Problema: "Error al desregistrar equipo"

**Síntomas:**
- El botón de desregistro no aparece
- Error al intentar eliminar configuración

**Soluciones:**

1. **Verificar permisos de administrador:**
   - El botón solo aparece si inicias sesión con rol "Admin"

2. **Eliminar manualmente:**
   ```powershell
   # Como administrador
   Remove-Item -Path "C:\ProgramData\Unilocker\*" -Force
   Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "UnilockerClient" -ErrorAction SilentlyContinue
   ```

---

### Problema: "Ventana no aparece en pantalla completa"

**Síntomas:**
- LoginWindow no cubre toda la pantalla
- Se pueden ver otros programas detrás

**Soluciones:**

1. **Verificar resolución:**
   - El modo Maximized debería funcionar en cualquier resolución

2. **Forzar modo kiosco:**
   - La ventana ya tiene `WindowState="Maximized"` y `WindowStyle="None"`
   - Asegurar que `Topmost="True"` en LoginWindow

---

## 📊 Checklist de Despliegue

Usa esta lista para asegurar un despliegue exitoso:

### Antes de Compilar
- [ ] Código compila sin errores ni warnings
- [ ] Todas las features funcionan correctamente
- [ ] Se probó el flujo completo (registro → login → sesión → logout)
- [ ] Actualizar número de versión en el proyecto

### Durante la Compilación
- [ ] Limpiar compilaciones anteriores
- [ ] Publicar en modo Release
- [ ] Verificar que el .exe se genera correctamente
- [ ] Verificar tamaño del ejecutable (~100-150 MB es normal)

### Crear Instalador
- [ ] Actualizar versión en script de Inno Setup
- [ ] Compilar instalador sin errores
- [ ] Probar instalador en VM o equipo limpio

### Distribución
- [ ] Documentar URL de descarga
- [ ] Crear instrucciones para usuarios finales
- [ ] Notificar a administradores de sistemas
- [ ] Preparar soporte técnico

### Post-Instalación
- [ ] Verificar instalación en al menos 3 equipos de prueba
- [ ] Confirmar conexión con la API
- [ ] Verificar registro de equipos
- [ ] Verificar inicio automático
- [ ] Verificar modo kiosco funciona correctamente

---

## 📞 Soporte

Para problemas durante el despliegue:

1. Revisar logs de la aplicación (si están implementados)
2. Verificar conectividad de red
3. Consultar con el equipo de desarrollo
4. Crear issue en GitHub con detalles completos

---

## 📚 Recursos Adicionales

- [Documentación de dotnet publish](https://learn.microsoft.com/es-es/dotnet/core/tools/dotnet-publish)
- [Guía de Inno Setup](https://jrsoftware.org/ishelp/)
- [Deployment de aplicaciones WPF](https://learn.microsoft.com/es-es/dotnet/desktop/wpf/deployment/)

---

**Última actualización:** Diciembre 2025  
**Versión del documento:** 1.0  
**Autor:** Rommel Rodrigo Gutierrez Herrera
