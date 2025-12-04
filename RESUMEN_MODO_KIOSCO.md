# 🔒 Resumen Rápido: Modo Kiosco Implementado

## ✅ ¿Qué se hizo?

La aplicación Unilocker ahora funciona como un **sistema de kiosco de laboratorio** con las siguientes características:

### 1. 🚀 Auto Inicio con Windows
- La aplicación se ejecuta automáticamente al iniciar Windows
- Bloquea el acceso a la computadora hasta que se inicie sesión
- Scripts de instalación incluidos: `InstalarAutoInicio.ps1` y `DesinstalarAutoInicio.ps1`

### 2. 🔐 LoginWindow - Pantalla de Bloqueo
```
┌────────────────────────────────────────┐
│  🎓 UNILOCKER                          │
│  ⚠️ DEBES INICIAR SESIÓN              │
│                                        │
│  [Usuario: _________]                  │
│  [Contraseña: _______]                 │
│                                        │
│  [  INICIAR SESIÓN  ]                  │
│                                        │
│  ❌ NO PUEDES CERRAR ESTA VENTANA      │
└────────────────────────────────────────┘
```
**Características:**
- Pantalla completa sin bordes
- Siempre visible (Topmost)
- **NO se puede cerrar** con X, Alt+F4, ni Task Manager
- Muestra advertencia si intentas cerrar

### 3. 🖥️ MainWindow - Sesión Activa
```
┌────────────────────────────────────────┐
│  ✓ SESIÓN ACTIVA                       │
│                                        │
│  Usuario: Juan Pérez                   │
│  Aula: LAB-201                         │
│  Duración: 00:45:23                    │
│                                        │
│  [🚪 Cerrar Sesión]                    │
│                                        │
│  ❌ Solo se cierra con el botón        │
└────────────────────────────────────────┘
```
**Características:**
- Pantalla completa sin bordes
- Siempre visible (Topmost)
- **NO se puede cerrar** con X o Alt+F4
- **Solo se cierra** mediante el botón "Cerrar Sesión"

### 4. 📊 Flujo Completo

```
Windows Inicia
    ↓
🔒 Unilocker se ejecuta automáticamente
    ↓
🔐 LoginWindow (BLOQUEADO)
    ↓
Usuario ingresa credenciales
    ↓
📧 Código 2FA por email
    ↓
✓ Código verificado
    ↓
🖥️ MainWindow (DESBLOQUEADO)
    ↓
Usuario trabaja normalmente
    ↓
🚪 Click en "Cerrar Sesión"
    ↓
📝 Reportar problemas (opcional)
    ↓
✓ Sesión cerrada en BD
    ↓
🔄 Vuelve a LoginWindow
```

---

## 📁 Archivos Modificados

### 1. LoginWindow - Modo Kiosco
**Archivo:** `Unilocker.Client/Views/LoginWindow.xaml`
```xml
<Window WindowStyle="None"       <!-- Sin bordes ni título -->
        Topmost="True"            <!-- Siempre visible -->
        WindowState="Maximized"   <!-- Pantalla completa -->
        ResizeMode="NoResize"     <!-- No se puede redimensionar -->
        Closing="Window_Closing"> <!-- Interceptar cierre -->
```

**Archivo:** `Unilocker.Client/Views/LoginWindow.xaml.cs`
```csharp
private bool _allowClose = false; // Flag para permitir cierre

private void Window_Closing(object sender, CancelEventArgs e)
{
    if (!_allowClose)
    {
        e.Cancel = true; // BLOQUEAR cierre
        MessageBox.Show("⚠️ No puedes cerrar esta ventana...");
    }
}

private void OpenMainWindow()
{
    _allowClose = true; // PERMITIR cierre solo después de login exitoso
    var mainWindow = new MainWindow(...);
    mainWindow.Show();
    this.Close();
}
```

### 2. MainWindow - Modo Kiosco
**Archivo:** `Unilocker.Client/MainWindow.xaml`
```xml
<Window WindowStyle="None"       <!-- Sin bordes ni título -->
        Topmost="True"            <!-- Siempre visible -->
        WindowState="Maximized"   <!-- Pantalla completa -->
        ResizeMode="NoResize"     <!-- No se puede redimensionar -->
        Closing="Window_Closing"> <!-- Interceptar cierre -->
```

**Archivo:** `Unilocker.Client/MainWindow.xaml.cs`
```csharp
private void Window_Closing(object sender, CancelEventArgs e)
{
    // Permitir cierre solo en estos casos:
    if (_isClosingBySystem) return; // Apagado de Windows
    if (_isLoggingOut) return;      // Botón "Cerrar Sesión"

    // BLOQUEAR cualquier otro intento
    e.Cancel = true;
    MessageBox.Show("⛔ NO PUEDES CERRAR ESTA VENTANA...");
}
```

### 3. Scripts de Instalación
**Archivo:** `Scripts/InstalarAutoInicio.ps1`
- Configura auto inicio en el registro de Windows
- Agrega entrada en: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`
- Opcionalmente deshabilita Task Manager y botones de apagado

**Archivo:** `Scripts/DesinstalarAutoInicio.ps1`
- Elimina auto inicio del registro
- Restaura Task Manager y botones de apagado
- Vuelve la computadora a modo normal

---

## 🚀 Cómo Instalar

### Paso 1: Compilar en Release
```powershell
cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Unilocker.Client"
dotnet publish -c Release -r win-x64 --self-contained true
```

### Paso 2: Copiar a ubicación permanente
```powershell
Copy-Item -Path "bin\Release\net8.0-windows\win-x64\publish\*" -Destination "C:\Program Files\Unilocker\" -Recurse
```

### Paso 3: Ejecutar instalador (como ADMINISTRADOR)
```powershell
cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Scripts"
.\InstalarAutoInicio.ps1
```

### Paso 4: Reiniciar
```powershell
Restart-Computer
```

---

## 🔓 Cómo Desinstalar

### Método 1: Script Automático
```powershell
# Como ADMINISTRADOR
cd "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Scripts"
.\DesinstalarAutoInicio.ps1
Restart-Computer
```

### Método 2: Manual
1. Abrir `regedit`
2. Ir a: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`
3. Eliminar entrada `Unilocker`
4. Reiniciar

---

## ⚠️ Restricciones en Modo Kiosco

### ❌ NO SE PUEDE:
- Cerrar con botón X
- Cerrar con Alt+F4
- Cerrar con Task Manager (si está deshabilitado)
- Minimizar o mover la ventana
- Cambiar de aplicación sin iniciar sesión
- Apagar sin cerrar sesión

### ✅ SÍ SE PUEDE:
- Iniciar sesión con credenciales válidas
- Usar la computadora normalmente después de login
- Cerrar sesión con el botón "Cerrar Sesión"
- Reportar problemas técnicos al cerrar
- Apagar/Reiniciar después de cerrar sesión correctamente

---

## 📊 Casos de Uso

### Caso 1: Inicio Normal
```
1. Estudiante enciende la computadora
2. Windows inicia → Unilocker se ejecuta automáticamente
3. Aparece LoginWindow (pantalla completa)
4. Estudiante ingresa usuario y contraseña
5. Recibe código 2FA por email
6. Ingresa código → Login exitoso
7. Aparece MainWindow (puede usar la computadora)
8. Al terminar: Click en "Cerrar Sesión"
9. Reporta si hubo problemas (opcional)
10. Vuelve a LoginWindow → Listo para el siguiente usuario
```

### Caso 2: Intento de Cerrar Aplicación
```
1. Usuario en MainWindow intenta cerrar con X
2. Aparece mensaje: "⛔ NO PUEDES CERRAR ESTA VENTANA"
3. Debe usar el botón "Cerrar Sesión"
```

### Caso 3: Apagado del Sistema
```
1. Usuario cierra sesión normalmente
2. Aparece LoginWindow
3. Ahora SÍ puede apagar desde el botón de Windows
   (o si tiene permisos, desde Ctrl+Alt+Del)
```

---

## 🛡️ Seguridad

### Niveles de Seguridad

**Nivel 1: Básico (Sin restricciones adicionales)**
- Auto inicio configurado
- Ventanas sin bordes y topmost
- Cierre bloqueado en código

**Nivel 2: Avanzado (Con restricciones adicionales)**
- Todo lo del Nivel 1, más:
- Task Manager deshabilitado
- Botones de apagado ocultos
- Cambio de usuario deshabilitado

### Acceso de Emergencia

Si necesitas acceso de emergencia:

1. **Modo Seguro de Windows:**
   - Reiniciar y presionar F8
   - Seleccionar "Modo Seguro"
   - Ejecutar script de desinstalación

2. **Cuenta de Administrador de Windows:**
   - Iniciar con cuenta admin local
   - Deshabilitar auto inicio manualmente

3. **Kill Switch de Desarrollo (solo desarrollo):**
   - Agregar código para salir con Ctrl+Alt+Shift+K
   - Solo para testing, no en producción

---

## 📚 Documentación Completa

Ver archivos:
- `MODO_KIOSCO.md` - Documentación completa del modo kiosco
- `SOLUCION_SESIONES.md` - Gestión de sesiones y cierre forzado
- `README.md` - Documentación general del proyecto

---

## ✅ Estado de Implementación

| Característica | Estado |
|----------------|--------|
| LoginWindow sin bordes | ✅ Implementado |
| LoginWindow topmost | ✅ Implementado |
| LoginWindow bloquear cierre | ✅ Implementado |
| MainWindow sin bordes | ✅ Implementado |
| MainWindow topmost | ✅ Implementado |
| MainWindow bloquear cierre | ✅ Implementado |
| Script auto inicio | ✅ Implementado |
| Script desinstalación | ✅ Implementado |
| Documentación completa | ✅ Implementado |
| Compilación sin errores | ✅ Verificado |

---

**🎯 RESULTADO:** La aplicación Unilocker ahora funciona como un sistema completo de control de acceso a laboratorios, bloqueando el uso de las computadoras hasta que los usuarios inicien sesión correctamente.

**Fecha:** 3 de diciembre de 2025  
**Rama:** feature-auth-sessions-reports  
**Sprint:** 1 - Autenticación, Sesiones y Reportes
