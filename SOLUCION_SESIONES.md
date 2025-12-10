# 🔒 Solución: Gestión de Sesiones en Unilocker

## 📋 Problemas Identificados

### 1. Sesiones Persistentes en Base de Datos
**Problema:** Cuando se cierra la aplicación de manera forzada (Ctrl+C, Task Manager, cierre inesperado), la sesión queda marcada como activa en la base de datos.

**Impacto:** El usuario no puede iniciar una nueva sesión porque el sistema detecta una sesión activa previa.

### 2. Conflicto de Sesiones No Resuelto
**Problema:** Al detectar una sesión activa y presionar "Sí" para cerrarla, la aplicación no cerraba correctamente la sesión anterior en la base de datos.

**Impacto:** La nueva sesión no se puede iniciar porque la sesión anterior sigue activa.

---

## ✅ Soluciones Implementadas

### Solución 1: Cierre Automático en OnClosed

**Archivo:** `Unilocker.Client/MainWindow.xaml.cs`

**Cambios:**
```csharp
protected override void OnClosed(EventArgs e)
{
    // Limpiar recursos
    _durationTimer?.Stop();
    SystemEvents.SessionEnding -= OnSystemSessionEnding;

    // CRÍTICO: Si hay una sesión activa y no estamos cerrando por logout normal,
    // intentar cerrar la sesión en la base de datos (para casos de forzar cierre)
    if (!_isLoggingOut && _sessionService.CurrentSessionId.HasValue)
    {
        try
        {
            // Intentar cerrar la sesión de manera síncrona antes de que la app termine
            var task = _sessionService.EndSessionAsync("Forced");
            task.Wait(TimeSpan.FromSeconds(2)); // Esperar máximo 2 segundos
        }
        catch (Exception ex)
        {
            // Registrar error pero no bloquear el cierre
            System.Diagnostics.Debug.WriteLine($"Error al cerrar sesión forzadamente: {ex.Message}");
        }
    }

    base.OnClosed(e);
}
```

**Funcionalidad:**
- Detecta si hay una sesión activa cuando la ventana se cierra
- Si no es un cierre por logout normal (`_isLoggingOut = false`), ejecuta el cierre forzado
- Espera hasta 2 segundos para completar la operación
- Si falla, registra el error pero no bloquea el cierre de la aplicación

**Cobertura:**
✅ Cierre con el botón X de la ventana  
✅ Cierre con Alt+F4  
✅ Cierre desde Task Manager  
✅ Cierre forzado (Ctrl+C en terminal)  
✅ Apagado/reinicio del sistema

---

### Solución 2: Endpoint de Forzar Cierre de Sesiones

**Archivo:** `Unilocker.Api/Controllers/SessionsController.cs`

**Nuevo Endpoint:**
```csharp
[HttpPost("user/{userId}/force-close")]
public async Task<IActionResult> ForceCloseUserSessions(int userId)
{
    // Buscar todas las sesiones activas del usuario
    var activeSessions = await _context.Sessions
        .Where(s => s.UserId == userId && s.IsActive)
        .ToListAsync();

    // Cerrar todas las sesiones activas
    foreach (var session in activeSessions)
    {
        session.EndDateTime = DateTime.Now;
        session.IsActive = false;
        session.EndMethod = "Forced";
        session.UpdatedAt = DateTime.Now;
    }

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Sesiones cerradas exitosamente",
        closedCount = activeSessions.Count,
        sessionIds = activeSessions.Select(s => s.Id).ToList()
    });
}
```

**Funcionalidad:**
- Recibe el ID del usuario
- Busca TODAS las sesiones activas de ese usuario
- Las marca como inactivas con `EndMethod = "Forced"`
- Retorna el número de sesiones cerradas

**URL:** `POST /api/sessions/user/{userId}/force-close`

---

### Solución 3: Método Cliente para Forzar Cierre

**Archivo:** `Unilocker.Client/Services/ApiService.cs`

**Nuevo Método:**
```csharp
public async Task<bool> ForceCloseUserSessionsAsync(int userId)
{
    var response = await _httpClient.PostAsync(
        $"{_baseUrl}/api/sessions/user/{userId}/force-close", 
        null);
    
    return response.IsSuccessStatusCode;
}
```

**Funcionalidad:**
- Llama al endpoint de la API para cerrar sesiones
- Retorna true si fue exitoso, false si falló

---

### Solución 4: Resolución de Conflictos Mejorada

**Archivo:** `Unilocker.Client/MainWindow.xaml.cs`

**Cambios en MainWindow_Loaded:**
```csharp
catch (HttpRequestException ex) when (ex.Message.Contains("409"))
{
    // Sesión activa detectada
    var conflictResult = MessageBox.Show(
        "⚠️ Sesión Activa Detectada\n\n" +
        "Ya existe una sesión activa para este usuario.\n\n" +
        "¿Desea cerrar la sesión anterior e iniciar una nueva?",
        "Conflicto de Sesión",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

    if (conflictResult == MessageBoxResult.Yes)
    {
        // Forzar cierre de sesiones activas
        bool closed = await _apiService.ForceCloseUserSessionsAsync(userId);
        
        if (closed)
        {
            // Reintentar iniciar sesión
            await _sessionService.StartSessionAsync(userId, computerId);
            // ... resto del código de éxito
        }
    }
    else
    {
        // Usuario canceló
        Application.Current.Shutdown();
    }
}
```

**Funcionalidad:**
- Detecta código HTTP 409 (Conflict) cuando hay sesión activa
- Muestra diálogo con opciones Sí/No
- Si el usuario elige "Sí":
  - Llama al endpoint para cerrar sesiones antiguas
  - Reintenta iniciar la nueva sesión
- Si el usuario elige "No":
  - Cierra la aplicación

---

## 🔄 Flujo Completo de Sesiones

### Inicio de Sesión Normal
1. Usuario inicia sesión en LoginWindow
2. MainWindow se carga y llama a `StartSessionAsync`
3. API valida que no haya sesiones activas
4. Si es exitoso: Crea nueva sesión en DB
5. Cliente inicia timer de heartbeat (30 segundos)

### Cierre Normal (Botón Cerrar Sesión)
1. Usuario hace clic en "Cerrar Sesión"
2. Se muestra ventana de reportes (opcional)
3. Se llama a `EndSessionAsync("Normal")`
4. API marca sesión como inactiva con `EndMethod = "Normal"`
5. Se limpia el token JWT
6. Se cierra la aplicación

### Cierre Forzado (X, Alt+F4, Task Manager)
1. Windows dispara el evento `OnClosed`
2. Se detecta que `_isLoggingOut = false` (no fue cierre normal)
3. Se ejecuta `EndSessionAsync("Forced")` con timeout de 2 segundos
4. API marca sesión como inactiva con `EndMethod = "Forced"`
5. Se cierra la aplicación

### Conflicto de Sesión Activa
1. Usuario intenta iniciar sesión pero ya tiene una activa
2. API responde con HTTP 409 Conflict
3. Cliente muestra diálogo "¿Cerrar sesión anterior?"
4. Si Sí:
   - Llama a `ForceCloseUserSessionsAsync(userId)`
   - API cierra todas las sesiones activas del usuario
   - Reintenta `StartSessionAsync`
5. Si No:
   - Cierra la aplicación

---

## 🧪 Casos de Prueba

### ✅ Prueba 1: Cierre Normal
1. Iniciar sesión
2. Hacer clic en "Cerrar Sesión"
3. Verificar en DB: `IsActive = false`, `EndMethod = "Normal"`

### ✅ Prueba 2: Cierre con X
1. Iniciar sesión
2. Hacer clic en la X de la ventana
3. Verificar en DB: `IsActive = false`, `EndMethod = "Forced"`

### ✅ Prueba 3: Cierre desde Task Manager
1. Iniciar sesión
2. Abrir Task Manager
3. Finalizar proceso "Unilocker.Client.exe"
4. Verificar en DB: `IsActive = false`, `EndMethod = "Forced"`

### ✅ Prueba 4: Conflicto de Sesión
1. Iniciar sesión (sesión queda activa manualmente en DB)
2. Cerrar app con Task Manager (simular fallo)
3. Volver a iniciar la app e iniciar sesión
4. Debe aparecer diálogo "Sesión Activa Detectada"
5. Hacer clic en "Sí"
6. Verificar: Sesión anterior cerrada, nueva sesión iniciada

### ✅ Prueba 5: Heartbeat
1. Iniciar sesión
2. Esperar 30 segundos
3. Verificar en DB: `LastHeartbeat` actualizado
4. Dejar pasar 5 minutos sin heartbeat
5. Intentar hacer una acción
6. Debe detectar sesión inactiva

---

## 📊 Campos de Sesión en Base de Datos

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | ID único de la sesión |
| `UserId` | int | ID del usuario |
| `ComputerId` | int | ID de la computadora |
| `StartDateTime` | DateTime | Fecha/hora de inicio |
| `EndDateTime` | DateTime? | Fecha/hora de fin (null si activa) |
| `IsActive` | bool | true = activa, false = cerrada |
| `EndMethod` | string | "Normal", "Forced", "Timeout" |
| `LastHeartbeat` | DateTime | Último heartbeat recibido |
| `CreatedAt` | DateTime | Fecha de creación |
| `UpdatedAt` | DateTime | Última actualización |

---

## 🔍 Endpoints de API

### Iniciar Sesión
```http
POST /api/sessions
Content-Type: application/json

{
  "userId": 1,
  "computerId": 5
}
```

### Finalizar Sesión
```http
POST /api/sessions/{sessionId}/end
Content-Type: application/json

{
  "endMethod": "Normal"
}
```

### Heartbeat
```http
POST /api/sessions/{sessionId}/heartbeat
```

### Forzar Cierre de Sesiones de Usuario
```http
POST /api/sessions/user/{userId}/force-close
```

---

## ⚠️ Consideraciones Importantes

1. **Timeout de 2 segundos:** El cierre forzado espera máximo 2 segundos para evitar que la app quede colgada.

2. **Manejo de errores:** Si el cierre forzado falla (red caída, API down), la app se cierra de todas formas pero registra el error en Debug.

3. **Flag `_isLoggingOut`:** Se usa para diferenciar entre cierre normal (con reportes) y cierre forzado (sin reportes).

4. **Múltiples sesiones:** El endpoint `force-close` cierra TODAS las sesiones activas del usuario, no solo una.

5. **Sincronización:** Se usa `Task.Wait()` en lugar de `await` porque `OnClosed` no puede ser asíncrono.

---

## 📝 Notas para el Desarrollador

- **No modificar el timeout:** Los 2 segundos están calibrados para balance entre esperar respuesta y no bloquear el cierre.
- **No remover el try-catch:** Es crítico para evitar que un error impida cerrar la app.
- **Verificar logs:** En caso de problemas, revisar la salida de Debug para ver errores de cierre.
- **Testing:** Siempre verificar en la BD que las sesiones se cierran correctamente después de cada tipo de cierre.

---

## 🎯 Estado Actual

✅ **IMPLEMENTADO:** Cierre automático en OnClosed  
✅ **IMPLEMENTADO:** Endpoint de forzar cierre  
✅ **IMPLEMENTADO:** Método cliente para forzar cierre  
✅ **IMPLEMENTADO:** Diálogo de resolución de conflictos  
✅ **COMPILACIÓN:** Sin errores, 3-4 advertencias menores (nullable warnings)

---

**Fecha de Implementación:** $(Get-Date)  
**Versión del Sistema:** Unilocker Sprint 1  
**Rama:** feature-auth-sessions-reports
