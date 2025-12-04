# ============================================================
# Script de Inicio Rápido - Unilocker Web + API
# ============================================================
# Este script inicia la API y abre Live Server automáticamente
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   INICIANDO UNILOCKER WEB + API" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en la carpeta correcta
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# 1. Iniciar API en nueva terminal
Write-Host "✓ Iniciando API en puerto 5013..." -ForegroundColor Green
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$scriptPath\Unilocker.Api'; Write-Host '🚀 Iniciando API...' -ForegroundColor Cyan; dotnet run"

# Esperar un poco para que la API inicie
Write-Host "  Esperando 5 segundos para que la API inicie..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# 2. Abrir VS Code en la carpeta Unilocker.Web
Write-Host "✓ Abriendo VS Code en Unilocker.Web..." -ForegroundColor Green
code "$scriptPath\Unilocker.Web"

Start-Sleep -Seconds 2

# 3. Instrucciones para el usuario
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   SIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. En VS Code, abre el archivo:" -ForegroundColor White
Write-Host "   Unilocker.Web/login.html" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Haz clic derecho en el archivo y selecciona:" -ForegroundColor White
Write-Host "   'Open with Live Server'" -ForegroundColor Cyan
Write-Host ""
Write-Host "   O presiona: Alt + L, luego Alt + O" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Se abrirá automáticamente en:" -ForegroundColor White
Write-Host "   http://localhost:5500/login.html" -ForegroundColor Green
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "La API está corriendo en: http://localhost:5013" -ForegroundColor Green
Write-Host ""

Pause
