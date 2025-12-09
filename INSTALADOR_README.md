# 🚀 Guía Rápida de Instalación - Unilocker Client

## Para Administradores de TI

### Generar el Instalador

1. **Instalar Inno Setup 6**:
   - Descargar de: https://jrsoftware.org/isdl.php
   - Instalar en la ruta por defecto

2. **Ejecutar el script de build**:
   ```powershell
   cd "c:\Proyecto de sistemas-Unilocker\UnilockerProyecto"
   .\Build-Installer.ps1 -Version "1.0.0"
   ```

3. **El instalador se generará en**: `.\installer\UnilockerClientSetup_v1.0.0.exe`

### Opciones del Script

```powershell
# Build completo con instalador
.\Build-Installer.ps1 -Version "1.0.0"

# Solo crear instalador (sin recompilar)
.\Build-Installer.ps1 -Version "1.0.0" -SkipBuild

# Crear también versión portable ZIP
.\Build-Installer.ps1 -Version "1.0.0" -CreatePortable
```

---

## Para Usuarios Finales

### Instalación

1. **Ejecutar** `UnilockerClientSetup_v1.0.0.exe`
2. **Aceptar** permisos de administrador (UAC)
3. **Seguir** el asistente:
   - ✅ Marcar "Ejecutar al iniciar Windows"
   - ✅ Crear icono en escritorio (opcional)
4. **Configurar** en el primer inicio:
   - Ingresar URL de la API: `http://192.168.0.5:5013` (ejemplo)
   - Probar conexión
   - Registrar el equipo (seleccionar aula/laboratorio)
5. **Reiniciar** la aplicación e iniciar sesión

### Requisitos del Sistema

- Windows 10/11 (64-bit)
- Conexión de red al servidor de la API
- Permisos de administrador para la instalación

### Modo Kiosco

La aplicación funciona en modo kiosco:
- 🔒 **Antes del login**: No se puede cerrar ni minimizar
- ✅ **Después del login**: Se puede minimizar pero NO cerrar
- 🚪 **Solo cierra** con el botón "Cerrar Sesión"
- 🔄 **Inicia automáticamente** con Windows

---

## Solución Rápida de Problemas

### "No se puede conectar a la API"

**Solución**:
- Verificar que la API esté corriendo
- Verificar firewall (puerto 5013)
- Editar manualmente: `C:\Program Files\Unilocker\appsettings.json`

### "No aparece al iniciar Windows"

**Solución**:
```powershell
# Agregar manualmente al inicio
$exe = "C:\Program Files\Unilocker\Unilocker.Client.exe"
Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "UnilockerClient" -Value "`"$exe`""
```

### "Necesito desregistrar el equipo"

**Solución**:
- Iniciar sesión como administrador
- Click en "Desregistrar Equipo" (botón visible solo para admins)

---

## Documentación Completa

Ver **DESPLIEGUE_PRODUCCION.md** para documentación detallada.

## Soporte

Universidad Privada del Valle  
Sistema Unilocker - Control de Laboratorios
