# ============================================================
# Script de Instalación - Unilocker Auto Inicio
# ============================================================
# Este script configura Unilocker para iniciarse automáticamente
# con Windows en modo kiosco (laboratorio).
#
# IMPORTANTE: Ejecutar como ADMINISTRADOR
# ============================================================

# Verificar que se ejecuta como administrador
$esAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $esAdmin) {
    Write-Host "❌ ERROR: Este script debe ejecutarse como ADMINISTRADOR" -ForegroundColor Red
    Write-Host ""
    Write-Host "Haz clic derecho en PowerShell y selecciona 'Ejecutar como administrador'" -ForegroundColor Yellow
    Write-Host ""
    Pause
    exit
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   INSTALADOR DE AUTO INICIO - UNILOCKER" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar ruta del ejecutable
Write-Host "Por favor, proporciona la ruta completa del ejecutable de Unilocker" -ForegroundColor Yellow
Write-Host "Ejemplo: C:\Unilocker\Unilocker.Client.exe" -ForegroundColor Gray
Write-Host ""

$exePath = Read-Host "Ruta del ejecutable"

# Verificar que el archivo existe
if (-not (Test-Path $exePath)) {
    Write-Host ""
    Write-Host "❌ ERROR: El archivo no existe en la ruta especificada" -ForegroundColor Red
    Write-Host "Verifica la ruta e intenta nuevamente" -ForegroundColor Yellow
    Write-Host ""
    Pause
    exit
}

Write-Host ""
Write-Host "✓ Archivo encontrado: $exePath" -ForegroundColor Green
Write-Host ""

# Configurar Auto Inicio
Write-Host "Configurando auto inicio..." -ForegroundColor Cyan

# Ruta del registro para auto inicio
$registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
$valueName = "Unilocker"

try {
    # Agregar al registro
    Set-ItemProperty -Path $registryPath -Name $valueName -Value $exePath -Type String
    
    Write-Host "✓ Auto inicio configurado exitosamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "La aplicación Unilocker ahora se iniciará automáticamente con Windows" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ ERROR al configurar auto inicio: $_" -ForegroundColor Red
    Write-Host ""
    Pause
    exit
}

# Opcional: Deshabilitar Alt+F4, Ctrl+Alt+Del, etc.
Write-Host ""
Write-Host "¿Deseas aplicar restricciones adicionales de seguridad?" -ForegroundColor Yellow
Write-Host "Esto deshabilitará:" -ForegroundColor Gray
Write-Host "  • Task Manager (Administrador de Tareas)" -ForegroundColor Gray
Write-Host "  • Cambio de usuario" -ForegroundColor Gray
Write-Host "  • Opciones de apagado desde Ctrl+Alt+Del" -ForegroundColor Gray
Write-Host ""
$aplicarRestricciones = Read-Host "¿Aplicar restricciones? (S/N)"

if ($aplicarRestricciones -eq "S" -or $aplicarRestricciones -eq "s") {
    Write-Host ""
    Write-Host "Aplicando restricciones de seguridad..." -ForegroundColor Cyan
    
    try {
        # Deshabilitar Task Manager
        $policyPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\System"
        if (-not (Test-Path $policyPath)) {
            New-Item -Path $policyPath -Force | Out-Null
        }
        Set-ItemProperty -Path $policyPath -Name "DisableTaskMgr" -Value 1 -Type DWord
        
        Write-Host "✓ Task Manager deshabilitado" -ForegroundColor Green
        
        # Ocultar botón de apagado
        Set-ItemProperty -Path $policyPath -Name "ShutdownWithoutLogon" -Value 0 -Type DWord
        
        Write-Host "✓ Botón de apagado ocultado" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "✓ Restricciones aplicadas exitosamente" -ForegroundColor Green
    } catch {
        Write-Host "❌ ERROR al aplicar restricciones: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   INSTALACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Unilocker está configurado en modo KIOSCO:" -ForegroundColor Yellow
Write-Host "  ✓ Se inicia automáticamente con Windows" -ForegroundColor Green
Write-Host "  ✓ Pantalla completa (sin bordes)" -ForegroundColor Green
Write-Host "  ✓ No se puede cerrar con X o Alt+F4" -ForegroundColor Green
Write-Host "  ✓ Siempre visible (Topmost)" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANTE:" -ForegroundColor Red
Write-Host "  • Reinicia la computadora para que los cambios surtan efecto" -ForegroundColor Yellow
Write-Host "  • Al iniciar, se mostrará la pantalla de login" -ForegroundColor Yellow
Write-Host "  • Los usuarios DEBEN iniciar sesión para usar la computadora" -ForegroundColor Yellow
Write-Host ""

Pause
