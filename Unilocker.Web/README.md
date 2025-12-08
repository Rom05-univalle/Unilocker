# 🌐 Unilocker Web - Instrucciones de Ejecución

## 📋 Pasos para ejecutar el Frontend Web

### Opción 1: Inicio Manual (Recomendado)

1. **Iniciar la API:**
   ```powershell
   cd Unilocker.Api
   dotnet run
   ```
   Espera a ver: `Now listening on: http://localhost:5013`

2. **Iniciar Live Server:**
   - Abre cualquier archivo HTML en `Unilocker.Web/`
   - Haz clic derecho → **"Open with Live Server"**
   - O presiona `Alt + L` seguido de `Alt + O`
   - Se abrirá en: `http://localhost:5500`

3. **Abrir el Login:**
   - Navega a: `http://localhost:5500/Unilocker.Web/login.html`

### Opción 2: Usando F5 (Requiere Live Server corriendo)

1. **PRIMERO** inicia Live Server manualmente (paso 2 de arriba)
2. Luego presiona F5 en VS Code
3. Se ejecutará la API automáticamente y abrirá Chrome

## ⚙️ Configuración

- **Puerto de la API:** `5013` (configurado en `js/api.js` y `js/auth.js`)
- **Puerto de Live Server:** `5500` (configurado en `.vscode/settings.json`)

## 🔐 Usuario de Prueba

Usa las credenciales de un usuario existente en tu base de datos.

## 📄 Páginas Disponibles

- `login.html` - Inicio de sesión
- `dashboard.html` - Panel principal
- `sessions.html` - Sesiones activas
- `reports.html` - Reportes de problemas
- `computers.html` - Gestión de computadoras
- `classrooms.html` - Gestión de aulas
- `branches.html` - Gestión de sedes
- `users.html` - Gestión de usuarios
- `roles.html` - Gestión de roles
- `problemtypes.html` - Tipos de problemas
- `audit.html` - Auditoría
- `blocks.html` - Bloqueos

## 🐛 Solución de Problemas

### "ERR_CONNECTION_REFUSED" en localhost:5500
**Causa:** Live Server no está corriendo  
**Solución:** Inicia Live Server manualmente (clic derecho → Open with Live Server)

### La API no responde
**Causa:** La API no está corriendo o está en otro puerto  
**Solución:** 
```powershell
cd Unilocker.Api
dotnet run
```

### Errores de CORS
**Causa:** La API rechaza peticiones del navegador  
**Solución:** Ya está configurado CORS en `Program.cs` para localhost:5500

## 📌 Comando Rápido

Abre 2 terminales en VS Code:

**Terminal 1 - API:**
```powershell
cd Unilocker.Api; dotnet run
```

**Terminal 2 - Web:**
```powershell
# Luego abre cualquier HTML y usa "Open with Live Server"
```
