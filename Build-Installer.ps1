# ============================================
# Script de Build y Empaquetado - Unilocker Client
# ============================================
# Este script automatiza:
# 1. Limpieza de builds anteriores
# 2. Compilación en modo Release
# 3. Publicación self-contained
# 4. Creación del instalador con Inno Setup
# ============================================

param(
    [string]$Version = "1.0.0",
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [switch]$CreatePortable
)

$ErrorActionPreference = "Stop"

# Colores para output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

Write-Info "================================================"
Write-Info "  UNILOCKER CLIENT - BUILD & PACKAGE SCRIPT"
Write-Info "  Versión: $Version"
Write-Info "================================================"
Write-Host ""

# ============================================
# 1. VERIFICAR REQUISITOS
# ============================================
Write-Info "[1/6] Verificando requisitos..."

# Verificar dotnet CLI
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "❌ .NET SDK no está instalado o no está en el PATH"
    exit 1
}

$dotnetVersion = dotnet --version
Write-Success "  ✓ .NET SDK encontrado: $dotnetVersion"

# Verificar Inno Setup (si no se salta el instalador)
if (!$SkipInstaller) {
    $innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (!(Test-Path $innoSetupPath)) {
        Write-Warning "  ⚠ Inno Setup no encontrado en la ubicación por defecto"
        Write-Warning "  Buscando en rutas alternativas..."
        
        $alternativePaths = @(
            "C:\Program Files\Inno Setup 6\ISCC.exe",
            "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        )
        
        $found = $false
        foreach ($path in $alternativePaths) {
            if (Test-Path $path) {
                $innoSetupPath = $path
                $found = $true
                break
            }
        }
        
        if (!$found) {
            Write-Warning "  ⚠ Inno Setup no encontrado. Instalador se omitirá."
            $SkipInstaller = $true
        }
    }
    
    if (!$SkipInstaller) {
        Write-Success "  ✓ Inno Setup encontrado: $innoSetupPath"
    }
}

Write-Host ""

# ============================================
# 2. LIMPIAR BUILDS ANTERIORES
# ============================================
if (!$SkipBuild) {
    Write-Info "[2/6] Limpiando builds anteriores..."

    $cleanPaths = @(
        ".\Unilocker.Client\bin\Release",
        ".\Unilocker.Client\obj\Release",
        ".\Unilocker.Client\publish",
        ".\installer"
    )

    foreach ($path in $cleanPaths) {
        if (Test-Path $path) {
            Remove-Item -Recurse -Force $path -ErrorAction SilentlyContinue
            Write-Success "  ✓ Eliminado: $path"
        }
    }

    Write-Host ""
}

# ============================================
# 3. COMPILAR PROYECTO
# ============================================
if (!$SkipBuild) {
    Write-Info "[3/6] Compilando proyecto..."

    Push-Location ".\Unilocker.Client"

    try {
        # Clean
        Write-Info "  Ejecutando dotnet clean..."
        dotnet clean -c Release | Out-Null

        # Restore
        Write-Info "  Ejecutando dotnet restore..."
        dotnet restore | Out-Null

        # Build
        Write-Info "  Ejecutando dotnet build..."
        $buildOutput = dotnet build -c Release --no-restore 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Error en la compilación:"
            Write-Host $buildOutput
            exit 1
        }

        Write-Success "  ✓ Compilación exitosa"
    }
    finally {
        Pop-Location
    }

    Write-Host ""
}

# ============================================
# 4. PUBLICAR APLICACIÓN
# ============================================
if (!$SkipBuild) {
    Write-Info "[4/6] Publicando aplicación..."

    Push-Location ".\Unilocker.Client"

    try {
        Write-Info "  Generando ejecutable self-contained..."
        Write-Info "  Configuración:"
        Write-Info "    - Target: win-x64"
        Write-Info "    - Mode: Self-contained"
        Write-Info "    - Single file: Yes"
        Write-Host ""

        $publishArgs = @(
            "publish",
            "-c", "Release",
            "-r", "win-x64",
            "--self-contained", "true",
            "-p:PublishSingleFile=true",
            "-p:IncludeNativeLibrariesForSelfExtract=true",
            "-p:EnableCompressionInSingleFile=true",
            "-o", ".\publish\win-x64"
        )

        $publishOutput = & dotnet $publishArgs 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Error en la publicación:"
            Write-Host $publishOutput
            exit 1
        }

        Write-Success "  ✓ Publicación exitosa"

        # Verificar archivos generados
        $exePath = ".\publish\win-x64\Unilocker.Client.exe"
        if (Test-Path $exePath) {
            $exeSize = (Get-Item $exePath).Length / 1MB
            Write-Success "  ✓ Ejecutable generado: Unilocker.Client.exe ($([math]::Round($exeSize, 2)) MB)"
        } else {
            Write-Error "❌ No se encontró el ejecutable generado"
            exit 1
        }

        if (Test-Path ".\publish\win-x64\appsettings.json") {
            Write-Success "  ✓ Archivo de configuración copiado"
        }
    }
    finally {
        Pop-Location
    }

    Write-Host ""
}

# ============================================
# 5. CREAR VERSIÓN PORTABLE (OPCIONAL)
# ============================================
if ($CreatePortable) {
    Write-Info "[5a/6] Creando versión portable..."

    $portableDir = ".\portable"
    $zipFile = ".\UnilockerClient_Portable_v$Version.zip"

    # Crear directorio
    if (!(Test-Path $portableDir)) {
        New-Item -ItemType Directory -Path $portableDir -Force | Out-Null
    }

    # Copiar archivos
    Copy-Item ".\Unilocker.Client\publish\win-x64\*" -Destination $portableDir -Recurse -Force

    # Crear README
    $readmeContent = @"
UNILOCKER CLIENT - VERSIÓN PORTABLE v$Version
==============================================

INSTALACIÓN:
1. Copia esta carpeta a la ubicación deseada (ej: C:\Program Files\Unilocker)
2. Ejecuta Unilocker.Client.exe
3. Sigue el asistente de configuración:
   - Configura la URL de la API
   - Registra el equipo en el sistema

REQUISITOS:
- Windows 10/11 (64-bit)
- Conexión de red al servidor de la API

CONFIGURACIÓN:
Los datos se guardan en:
- Configuración: C:\ProgramData\Unilocker
- Registro: appsettings.json (en esta carpeta)

MODO KIOSCO:
La aplicación está configurada para:
- Iniciar automáticamente con Windows
- Bloquear el acceso hasta que un usuario inicie sesión
- Permitir minimizar después del login
- Solo cerrar mediante el botón "Cerrar Sesión"

DESINSTALACIÓN:
1. Ejecutar la app como administrador
2. Iniciar sesión con usuario admin
3. Click en "Desregistrar Equipo"
4. Eliminar esta carpeta

SOPORTE:
Universidad Privada del Valle
Sistema Unilocker - Control de Laboratorios

Versión: $Version
Fecha: $(Get-Date -Format "yyyy-MM-dd")
"@

    $readmeContent | Out-File -FilePath "$portableDir\README.txt" -Encoding UTF8

    # Comprimir
    if (Test-Path $zipFile) {
        Remove-Item $zipFile -Force
    }

    Compress-Archive -Path "$portableDir\*" -DestinationPath $zipFile -Force

    Write-Success "  ✓ Versión portable creada: $zipFile"
    
    # Limpiar directorio temporal
    Remove-Item $portableDir -Recurse -Force

    Write-Host ""
}

# ============================================
# 6. CREAR INSTALADOR
# ============================================
if (!$SkipInstaller) {
    Write-Info "[6/6] Creando instalador..."

    if (!(Test-Path ".\UnilockerInstaller.iss")) {
        Write-Error "❌ No se encontró el script de Inno Setup: UnilockerInstaller.iss"
        exit 1
    }

    # Actualizar versión en el script
    Write-Info "  Actualizando versión en script de Inno Setup..."
    $issContent = Get-Content ".\UnilockerInstaller.iss" -Raw
    $issContent = $issContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$Version`""
    $issContent | Set-Content ".\UnilockerInstaller.iss" -Encoding UTF8

    Write-Info "  Compilando instalador con Inno Setup..."
    $innoOutput = & $innoSetupPath ".\UnilockerInstaller.iss" 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Error al crear el instalador:"
        Write-Host $innoOutput
        exit 1
    }

    # Verificar que se creó el instalador
    $installerFile = ".\installer\UnilockerClientSetup_v$Version.exe"
    if (Test-Path $installerFile) {
        $installerSize = (Get-Item $installerFile).Length / 1MB
        Write-Success "  ✓ Instalador creado: UnilockerClientSetup_v$Version.exe ($([math]::Round($installerSize, 2)) MB)"
    } else {
        Write-Error "❌ No se encontró el instalador generado"
        exit 1
    }

    Write-Host ""
}

# ============================================
# RESUMEN FINAL
# ============================================
Write-Info "================================================"
Write-Success "✅ BUILD COMPLETADO EXITOSAMENTE"
Write-Info "================================================"
Write-Host ""

Write-Info "📁 Archivos generados:"
Write-Host ""

if (!$SkipBuild) {
    Write-Host "  📦 Ejecutable:"
    Write-Host "     .\Unilocker.Client\publish\win-x64\Unilocker.Client.exe"
    Write-Host ""
}

if (!$SkipInstaller) {
    Write-Host "  💿 Instalador:"
    Write-Host "     .\installer\UnilockerClientSetup_v$Version.exe"
    Write-Host ""
}

if ($CreatePortable) {
    Write-Host "  🗜️ Versión Portable:"
    Write-Host "     .\UnilockerClient_Portable_v$Version.zip"
    Write-Host ""
}

Write-Info "📝 Próximos pasos:"
Write-Host "  1. Probar el instalador en un equipo limpio o VM"
Write-Host "  2. Verificar que la configuración inicial funciona correctamente"
Write-Host "  3. Verificar el registro del equipo"
Write-Host "  4. Verificar el modo kiosco (inicio automático y bloqueo)"
Write-Host "  5. Distribuir a los equipos de producción"
Write-Host ""

Write-Success "Listo para desplegar!"
Write-Host ""
