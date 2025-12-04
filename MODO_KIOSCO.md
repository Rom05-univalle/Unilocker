# 🔒 MODO KIOSCO - Unilocker

## 📋 ¿Qué es el Modo Kiosco?

El **modo kiosco** convierte la aplicación Unilocker en un sistema de control total de la computadora de laboratorio, donde:

- ✅ La aplicación se inicia **automáticamente** con Windows
- ✅ Los usuarios **DEBEN iniciar sesión** para usar la computadora
- ✅ **NO se puede cerrar** la aplicación (ni con X, ni con Alt+F4, ni Task Manager)
- ✅ La aplicación está **siempre visible** en pantalla completa
- ✅ Solo se puede cerrar mediante el botón **"Cerrar Sesión"**

---

## 🎯 Características Implementadas

### 1. LoginWindow - Pantalla de Bloqueo

**Características:**
- **Pantalla completa sin bordes** (`WindowStyle="None"`)
- **Siempre visible** (`Topmost="True"`)
- **No se puede cerrar** con X o Alt+F4
- **Bloquea el acceso** a la computadora hasta iniciar sesión
- Muestra mensaje de advertencia si intentas cerrarla

**Código clave:**
```xml
<Window WindowStyle="None"
        Topmost="True"
        ResizeMode="NoResize"
        WindowState="Maximized"
        Closing="Window_Closing">
```

```csharp
private void Window_Closing(object sender, CancelEventArgs e)
{
    if (!_allowClose)
    {
        e.Cancel = true; // Cancelar cierre
        MessageBox.Show("⚠️ No puedes cerrar esta ventana...");
    }
}
```

### 2. MainWindow - Sesión Activa

**Características:**
- **Pantalla completa sin bordes** (`WindowStyle="None"`)
- **Siempre visible** (`Topmost="True"`)
- **No se puede cerrar** con X o Alt+F4
- **Solo se cierra** con el botón "Cerrar Sesión"
- Muestra mensaje restrictivo si intentas cerrarla

**Código clave:**
```csharp
private async void Window_Closing(object sender, CancelEventArgs e)
{
    if (_isClosingBySystem) return; // Permitir cierre del sistema
    if (_isLoggingOut) return;      // Permitir cierre por logout

    // BLOQUEAR cualquier otro intento de cierre
    e.Cancel = true;
    MessageBox.Show("⛔ NO PUEDES CERRAR ESTA VENTANA...");
}
```

### 3. Auto Inicio con Windows

**Scripts de instalación proporcionados:**
- `InstalarAutoInicio.ps1` - Configura auto inicio
- `DesinstalarAutoInicio.ps1` - Elimina auto inicio

**Lo que hace el script:**
- Agrega Unilocker al registro de Windows (HKLM\Software\Microsoft\Windows\CurrentVersion\Run)
- Opcionalmente deshabilita Task Manager
- Oculta botones de apagado/cambio de usuario

---

## 📦 Instalación del Modo Kiosco

### Paso 1: Compilar la Aplicación en Release

```powershell
cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Unilocker.Client"
dotnet publish -c Release -r win-x64 --self-contained true
```

Esto genera el ejecutable en:
```
Unilocker.Client\bin\Release\net8.0-windows\win-x64\publish\Unilocker.Client.exe
```

### Paso 2: Copiar a Ubicación Permanente

Copia la carpeta `publish` completa a una ubicación permanente, por ejemplo:
```
C:\Program Files\Unilocker\
```

### Paso 3: Ejecutar Script de Instalación

1. Abre PowerShell **como ADMINISTRADOR**
2. Navega a la carpeta de scripts:
   ```powershell
   cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Scripts"
   ```
3. Ejecuta el instalador:
   ```powershell
   .\InstalarAutoInicio.ps1
   ```
4. Proporciona la ruta del ejecutable cuando se solicite:
   ```
   C:\Program Files\Unilocker\Unilocker.Client.exe
   ```
5. Decide si aplicar restricciones adicionales (Task Manager, etc.)

### Paso 4: Reiniciar la Computadora

Después de reiniciar, Unilocker se iniciará automáticamente y bloqueará el acceso.

---

## 🔓 Desinstalación del Modo Kiosco

### Si necesitas desactivar el modo kiosco:

1. **Método 1: Usar el Script de Desinstalación**
   ```powershell
   # Como ADMINISTRADOR
   cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Scripts"
   .\DesinstalarAutoInicio.ps1
   ```

2. **Método 2: Manual desde el Registro**
   - Presiona `Win + R`, escribe `regedit`
   - Navega a: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`
   - Elimina la entrada `Unilocker`
   - Reinicia la computadora

3. **Método 3: Habilitar Task Manager Manualmente**
   ```powershell
   # Como ADMINISTRADOR
   Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableTaskMgr"
   ```

---

## 🛡️ Seguridad y Restricciones

### Restricciones Aplicadas (Opcional)

Si elegiste aplicar restricciones adicionales durante la instalación:

| Restricción | Descripción | Registro |
|-------------|-------------|----------|
| **Task Manager Deshabilitado** | No se puede abrir con Ctrl+Shift+Esc | `HKCU\...\Policies\System\DisableTaskMgr = 1` |
| **Botón Apagado Oculto** | No aparece en Ctrl+Alt+Del | `HKCU\...\Policies\System\ShutdownWithoutLogon = 0` |

### Lo que NO se puede hacer en modo kiosco:

- ❌ Cerrar la aplicación con X
- ❌ Cerrar con Alt+F4
- ❌ Cerrar con Task Manager (si está deshabilitado)
- ❌ Cambiar de ventana sin iniciar sesión
- ❌ Apagar la computadora sin cerrar sesión

### Lo que SÍ se puede hacer:

- ✅ Iniciar sesión con credenciales válidas
- ✅ Usar la computadora normalmente después del login
- ✅ Cerrar sesión con el botón "Cerrar Sesión"
- ✅ Reportar problemas al cerrar sesión
- ✅ Apagar/Reiniciar después de cerrar sesión

---

## 🔧 Configuración Avanzada

### Deshabilitar Modo Kiosco Temporalmente (para pruebas)

Si estás desarrollando y necesitas modo normal:

1. **En `LoginWindow.xaml`:**
   ```xml
   <!-- Cambiar de: -->
   <Window WindowStyle="None" Topmost="True"...>
   
   <!-- A: -->
   <Window WindowStyle="SingleBorderWindow" Topmost="False"...>
   ```

2. **En `MainWindow.xaml`:**
   ```xml
   <!-- Cambiar de: -->
   <Window WindowStyle="None" Topmost="True"...>
   
   <!-- A: -->
   <Window WindowStyle="SingleBorderWindow" Topmost="False" ResizeMode="CanResize"...>
   ```

3. **En `LoginWindow.xaml.cs` y `MainWindow.xaml.cs`:**
   ```csharp
   // Comentar la línea que cancela el cierre:
   // e.Cancel = true;
   ```

### Auto Inicio Solo para Usuario Específico

Si solo quieres auto inicio para el usuario actual (no para todos):

1. Usa la ruta de registro:
   ```
   HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
   ```
   en lugar de:
   ```
   HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
   ```

2. Modifica el script `InstalarAutoInicio.ps1`:
   ```powershell
   $registryPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
   ```

---

## 📊 Flujo del Modo Kiosco

```
┌─────────────────────────────────────────────────┐
│  Windows Inicia                                 │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  Unilocker.Client.exe se ejecuta automáticamente│
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  LoginWindow (pantalla completa, sin cerrar)    │
│  ⚠️ BLOQUEADO - Debes iniciar sesión            │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
        Usuario ingresa credenciales
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  Verificación 2FA (código por email)            │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
           Código correcto
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  MainWindow (pantalla completa, sin cerrar)     │
│  ✓ Sesión activa - Computadora desbloqueada    │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
    Usuario trabaja normalmente
                  │
                  ▼
    Usuario hace clic en "Cerrar Sesión"
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  ReportWindow (opcional - reportar problemas)   │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  Sesión cerrada en BD                           │
│  Token JWT eliminado                            │
│  Aplicación se cierra                           │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  Unilocker.Client.exe se reinicia automáticamente│
│  Vuelve a LoginWindow                           │
└─────────────────────────────────────────────────┘
```

---

## ⚠️ Consideraciones Importantes

### Para Administradores:

1. **Acceso de Emergencia:** Siempre ten una cuenta de administrador de Windows para acceso de emergencia.

2. **Backup de Scripts:** Guarda los scripts de instalación/desinstalación en un lugar seguro.

3. **Modo Seguro:** Si necesitas desactivar Unilocker urgentemente, inicia Windows en Modo Seguro (F8 al arrancar).

4. **Desregistro de Equipos:** Los administradores pueden desregistrar equipos desde dentro de MainWindow (botón oculto para no-admins).

### Para Desarrolladores:

1. **NO uses modo kiosco en desarrollo:** Cambia `WindowStyle` y `Topmost` durante desarrollo.

2. **Testea en máquina virtual:** Prueba el modo kiosco en una VM antes de aplicarlo en producción.

3. **Ten un kill switch:** Considera agregar una combinación de teclas secreta (ej: Ctrl+Alt+Shift+K) para salir en emergencias durante desarrollo.

---

## 🐛 Troubleshooting

### Problema: No puedo cerrar la aplicación

**Solución:**
1. Usa el botón "Cerrar Sesión" dentro de la app
2. Si no responde: Reinicia desde el botón físico de la PC
3. En próximo inicio: Modo Seguro → Desinstalar auto inicio

### Problema: La app no se inicia automáticamente

**Verifica:**
1. Ruta del ejecutable en el registro es correcta
2. El ejecutable existe en esa ubicación
3. No hay errores en Event Viewer de Windows

### Problema: Task Manager sigue apareciendo

**Solución:**
1. Ejecuta nuevamente `InstalarAutoInicio.ps1` y elige "Sí" en restricciones
2. O manualmente: `Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\System" -Name "DisableTaskMgr" -Value 1`

---

## 📚 Referencias

- **Archivos modificados:**
  - `Unilocker.Client/Views/LoginWindow.xaml` - UI sin bordes, topmost
  - `Unilocker.Client/Views/LoginWindow.xaml.cs` - Prevenir cierre
  - `Unilocker.Client/MainWindow.xaml` - UI sin bordes, topmost
  - `Unilocker.Client/MainWindow.xaml.cs` - Solo cerrar por logout

- **Scripts de instalación:**
  - `Scripts/InstalarAutoInicio.ps1` - Configurar auto inicio
  - `Scripts/DesinstalarAutoInicio.ps1` - Remover auto inicio

- **Documentación relacionada:**
  - `SOLUCION_SESIONES.md` - Gestión de sesiones y cierre forzado
  - `README.md` - Documentación general del proyecto

---

**Última actualización:** 3 de diciembre de 2025  
**Versión:** Unilocker Sprint 1 - Modo Kiosco  
**Rama:** feature-auth-sessions-reports
