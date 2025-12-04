# ============================================================
# Script de Desinstalación - Unilocker Auto Inicio
# ============================================================
# Este script ELIMINA la configuración de auto inicio de Unilocker
# y RESTAURA las opciones normales de Windows
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
Write-Host "   DESINSTALADOR DE AUTO INICIO - UNILOCKER" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Confirmar desinstalación
Write-Host "⚠️  ADVERTENCIA" -ForegroundColor Yellow
Write-Host ""
Write-Host "Este script eliminará:" -ForegroundColor Red
Write-Host "  • El auto inicio de Unilocker" -ForegroundColor Gray
Write-Host "  • Las restricciones de seguridad (Task Manager, etc.)" -ForegroundColor Gray
Write-Host ""
$confirmar = Read-Host "¿Estás seguro que deseas continuar? (S/N)"

if ($confirmar -ne "S" -and $confirmar -ne "s") {
    Write-Host ""
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    Write-Host ""
    Pause
    exit
}

Write-Host ""
Write-Host "Eliminando configuración de auto inicio..." -ForegroundColor Cyan

# Ruta del registro para auto inicio
$registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
$valueName = "Unilocker"

try {
    # Eliminar del registro
    Remove-ItemProperty -Path $registryPath -Name $valueName -ErrorAction SilentlyContinue
    
    Write-Host "✓ Auto inicio eliminado" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ ERROR al eliminar auto inicio: $_" -ForegroundColor Red
    Write-Host ""
}

# Restaurar Task Manager y otras opciones
Write-Host "Restaurando opciones de Windows..." -ForegroundColor Cyan

try {
    # Habilitar Task Manager
    $policyPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\System"
    Remove-ItemProperty -Path $policyPath -Name "DisableTaskMgr" -ErrorAction SilentlyContinue
    
    Write-Host "✓ Task Manager habilitado" -ForegroundColor Green
    
    # Mostrar botón de apagado
    Remove-ItemProperty -Path $policyPath -Name "ShutdownWithoutLogon" -ErrorAction SilentlyContinue
    
    Write-Host "✓ Botón de apagado restaurado" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "✓ Opciones de Windows restauradas" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR al restaurar opciones: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   DESINSTALACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Unilocker ya NO se iniciará automáticamente con Windows" -ForegroundColor Yellow
Write-Host ""
Write-Host "Reinicia la computadora para que los cambios surtan efecto" -ForegroundColor Yellow
Write-Host ""

Pause
