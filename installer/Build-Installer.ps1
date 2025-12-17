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
$ProjectRoot = $PSScriptRoot
$ClientProject = Join-Path $ProjectRoot "Unilocker.Client"
$InstallerDir = Join-Path $ProjectRoot "installer"
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
if (Test-Path $InstallerDir) {
    Remove-Item "$InstallerDir\*" -Recurse -Force -Exclude "*.exe"
}
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
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

# Paso 3: Copiar archivos al directorio installer
Write-Host "[3/5] Copiando archivos al directorio installer..." -ForegroundColor Yellow
if (-not (Test-Path $InstallerDir)) {
    New-Item -ItemType Directory -Path $InstallerDir | Out-Null
}

# Copiar todos los archivos publicados
Copy-Item "$PublishDir\*" -Destination $InstallerDir -Recurse -Force

# Eliminar appsettings.json del installer (se creará dinámicamente)
$appSettingsInInstaller = Join-Path $InstallerDir "appsettings.json"
if (Test-Path $appSettingsInInstaller) {
    Remove-Item $appSettingsInInstaller -Force
}

Write-Host "      Archivos copiados correctamente" -ForegroundColor Green
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

# Paso 5: Limpiar archivos temporales
Write-Host "[5/5] Limpiando archivos temporales..." -ForegroundColor Yellow

# Eliminar archivos DLL, PDB y otros del directorio installer
Get-ChildItem $InstallerDir -Recurse | Where-Object {
    $_.Extension -in @('.dll', '.pdb', '.exe', '.json', '.config', '.xml') -and 
    $_.Name -ne "UnilockerClientSetup_v1.0.0.exe"
} | Remove-Item -Force

# Eliminar carpetas temporales
Get-ChildItem $InstallerDir -Directory | Remove-Item -Recurse -Force

# Eliminar .rar si existe
$rarFile = Join-Path $InstallerDir "UnilockerClientSetup_v1.0.0.rar"
if (Test-Path $rarFile) {
    Remove-Item $rarFile -Force
}

# Eliminar installer.rar de la raíz si existe
$rootRarFile = Join-Path $ProjectRoot "installer.rar"
if (Test-Path $rootRarFile) {
    Remove-Item $rootRarFile -Force
}

Write-Host "      Limpieza completada" -ForegroundColor Green
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
