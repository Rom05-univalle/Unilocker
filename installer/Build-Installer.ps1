# Script para compilar y crear el instalador de Unilocker Client
# Este script:
# 1. Compila el cliente en modo Release
# 2. Copia los archivos al directorio installer
# 3. Compila el instalador con Inno Setup
# 4. Limpia archivos temporales

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Unilocker Client - Build & Install " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Rutas
$InstallerDir = $PSScriptRoot
$ProjectRoot = Split-Path $InstallerDir -Parent
$ClientProject = Join-Path $ProjectRoot "Unilocker.Client"
$PublishDir = Join-Path $ClientProject "bin\Release\net8.0-windows\win-x64\publish"
$IssFile = Join-Path $InstallerDir "UnilockerInstaller.iss"
$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

# Verificar que existe Inno Setup
if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "ERROR: Inno Setup no encontrado en: $InnoSetupPath" -ForegroundColor Red
    Write-Host "Descarga e instala Inno Setup desde: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

# Paso 1: Limpiar directorios anteriores
Write-Host "[1/5] Limpiando directorios anteriores..." -ForegroundColor Yellow

# Eliminar instalador anterior
$oldInstaller = Join-Path $InstallerDir "UnilockerClientSetup_v1.0.0.exe"
if (Test-Path $oldInstaller) {
    Remove-Item $oldInstaller -Force
    Write-Host "      Instalador anterior eliminado" -ForegroundColor Green
}

# Limpiar directorio de publicación
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
    Write-Host "      Directorio de publicación limpiado" -ForegroundColor Green
}

# Limpiar archivos compilados anteriores del installer
if (Test-Path $InstallerDir) {
    Get-ChildItem $InstallerDir -Recurse | Where-Object {
        $_.Name -ne "UnilockerInstaller.iss" -and 
        $_.Name -ne "Build-Installer.ps1" -and 
        $_.Name -ne "README.md" -and
        -not $_.Name.StartsWith("UnilockerClientSetup")
    } | Remove-Item -Recurse -Force
    Write-Host "      Archivos compilados anteriores eliminados" -ForegroundColor Green
}

Write-Host "      Limpieza completada" -ForegroundColor Green
Write-Host ""

# Paso 2: Compilar el cliente (Self-Contained con runtime de .NET incluido)
Write-Host "[2/5] Compilando Unilocker.Client en modo Release (Self-Contained)..." -ForegroundColor Yellow
Write-Host "      Esto puede tardar varios minutos..." -ForegroundColor Cyan
Set-Location $ClientProject
$compileResult = dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR al compilar el cliente:" -ForegroundColor Red
    Write-Host $compileResult
    exit 1
}
Write-Host "      Compilación exitosa (incluye runtime de .NET 8)" -ForegroundColor Green
Write-Host ""

# Paso 3: Verificar directorio de publicación
Write-Host "[3/5] Verificando archivos compilados..." -ForegroundColor Yellow
if (-not (Test-Path $PublishDir)) {
    Write-Host "ERROR: Directorio de publicación no encontrado" -ForegroundColor Red
    exit 1
}

$fileCount = (Get-ChildItem $PublishDir -Recurse -File).Count
Write-Host "      Encontrados $fileCount archivos en $PublishDir" -ForegroundColor Green
Write-Host "      Inno Setup tomará los archivos directamente desde ahí" -ForegroundColor Cyan
Write-Host ""

# Paso 4: Compilar el instalador con Inno Setup
Write-Host "[4/5] Compilando instalador con Inno Setup..." -ForegroundColor Yellow
Set-Location $InstallerDir
& $InnoSetupPath $IssFile /Q
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR al compilar el instalador" -ForegroundColor Red
    exit 1
}
Write-Host "      Instalador creado exitosamente" -ForegroundColor Green
Write-Host ""

# Paso 5: Limpiar directorio installer
Write-Host "[5/5] Limpiando directorio installer..." -ForegroundColor Yellow

# Eliminar todos los archivos excepto los necesarios
Get-ChildItem $InstallerDir -Recurse | Where-Object {
    $_.Name -ne "UnilockerInstaller.iss" -and 
    $_.Name -ne "Build-Installer.ps1" -and 
    $_.Name -ne "README.md" -and
    -not $_.Name.StartsWith("UnilockerClientSetup")
} | Remove-Item -Recurse -Force

Write-Host "      Directorio limpiado - solo quedan archivos esenciales" -ForegroundColor Green
Write-Host ""

# Resumen
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Build completado exitosamente! " -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Instalador generado en:" -ForegroundColor Cyan
Write-Host "  $InstallerDir\UnilockerClientSetup_v1.0.0.exe" -ForegroundColor White
Write-Host ""
Write-Host "Características del instalador:" -ForegroundColor Cyan
Write-Host "  - Solicita URL de la API durante instalación" -ForegroundColor White
Write-Host "  - Opción de auto-inicio en Windows" -ForegroundColor White
Write-Host "  - Pregunta al desinstalar si eliminar datos" -ForegroundColor White
Write-Host "  - Crea acceso directo en escritorio" -ForegroundColor White
Write-Host ""

# Volver al directorio original
Set-Location $ProjectRoot
