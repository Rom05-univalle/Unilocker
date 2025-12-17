# Unilocker Web Dashboard

Dashboard administrativo web para el sistema Unilocker de control de acceso a laboratorios.

## Descripción

Interfaz web responsive para administración y monitoreo del sistema Unilocker. Permite gestionar usuarios, infraestructura, sesiones, reportes y visualizar estadísticas en tiempo real.

## Características

- Dashboard con KPIs en tiempo real
- Gestión completa de usuarios y roles
- Administración de infraestructura (Sucursales, Bloques, Aulas, Computadoras)
- Monitoreo de sesiones activas y cerradas
- Gestión de reportes de problemas
- Sistema de analíticas con gráficos
- Visualización de logs de auditoría
- Filtros avanzados en todos los módulos
- Totalizadores dinámicos
- Búsqueda y paginación
- Interfaz responsive (Bootstrap 5)

## Tecnologías

- HTML5
- CSS3
- JavaScript ES6+ (Vanilla JS)
- Bootstrap 5.3.2
- Chart.js 4.x para gráficos
- Font Awesome 6.5.1 para iconos
- Fetch API para comunicación con backend
- JWT para autenticación

## Estructura del Proyecto

```
Unilocker.Web/
├── index.html              # Página de inicio de sesión
├── dashboard.html          # Dashboard principal
├── users.html              # Gestión de usuarios
├── branches.html           # Gestión de sucursales
├── blocks.html             # Gestión de bloques
├── classrooms.html         # Gestión de aulas
├── computers.html          # Gestión de computadoras
├── sessions.html           # Monitoreo de sesiones
├── reports.html            # Gestión de reportes
├── analytics.html          # Analíticas y gráficos
├── audit.html              # Logs de auditoría
├── sidebar.html            # Barra lateral (componente)
├── css/
│   └── styles.css          # Estilos personalizados
├── js/
│   ├── api.js              # Configuración de API y fetch
│   ├── auth.js             # Lógica de autenticación
│   ├── ui.js               # Utilidades de UI
│   ├── users.js            # Módulo de usuarios
│   ├── branches.js         # Módulo de sucursales
│   ├── blocks.js           # Módulo de bloques
│   ├── classrooms.js       # Módulo de aulas
│   ├── computers.js        # Módulo de computadoras
│   ├── sessions.js         # Módulo de sesiones
│   ├── reports.js          # Módulo de reportes
│   ├── analytics.js        # Módulo de analíticas
│   ├── audit.js            # Módulo de auditoría
│   └── dashboard.js        # Módulo de dashboard
└── README.md
```

## Instalación

### Requisitos Previos
- API Backend ejecutándose (Unilocker.Api)
- Navegador web moderno (Chrome, Firefox, Edge)
- Servidor web local (opcional pero recomendado)

### Opción 1: Live Server (Recomendado)

1. Instalar extensión "Live Server" en VS Code

2. Abrir `Unilocker.Web/index.html`

3. Click derecho > "Open with Live Server"

4. El navegador abrirá automáticamente en `http://localhost:5500`

### Opción 2: Python HTTP Server

```bash
cd Unilocker.Web
python -m http.server 8080
```

Abrir navegador en `http://localhost:8080`

### Opción 3: Node.js HTTP Server

```bash
cd Unilocker.Web
npx http-server -p 8080
```

Abrir navegador en `http://localhost:8080`

## Configuración

### URL de la API

Editar `js/api.js` y cambiar la URL del backend:

```javascript
const API_BASE_URL = 'http://localhost:5013/api';
```

Para producción:
```javascript
const API_BASE_URL = 'http://192.168.1.100:5013/api';
```

## Inicio de Sesión

### Credenciales de Prueba

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| radmin | admin123 | Administrador |
| ruser | admin123 | Docente |
| estuser | admin123 | Estudiante |

### Flujo de Autenticación

1. Ingresar email y contraseña en `index.html`
2. El sistema envía `POST /api/auth/login`
3. Recibe token JWT y datos del usuario
4. Token se almacena en `localStorage`
5. Redirección a `dashboard.html`
6. Todas las peticiones incluyen el token en el header

## Módulos

### 1. Dashboard (dashboard.html)

**KPIs Principales:**
- Total de Infraestructuras (con listado de sucursales)
- Total de Laboratorios
- Total de Computadoras
- Computadoras Activas (porcentaje)
- Horas de Uso del mes
- Incidencias Abiertas (con desglose)

**Información del Sistema:**
- Total de Usuarios
- Total de Sesiones
- Última actualización

**Gráficos:**
- Uso por sucursal (barras)
- Uso por aula (horizontal)
- Sesiones activas resumen

### 2. Usuarios (users.html)

**Funciones:**
- Listar todos los usuarios
- Crear nuevo usuario
- Editar usuario existente
- Eliminar usuario (lógico)
- Filtro por rol
- Búsqueda por nombre/email

**Campos:**
- Nombre, Apellidos
- Email, Teléfono
- Username
- Contraseña
- Rol
- Estado (Activo/Inactivo)

### 3. Infraestructura

#### Sucursales (branches.html)
- Crear/Editar/Eliminar sucursales
- Código, Nombre, Dirección
- Ver bloques asociados

#### Bloques (blocks.html)
- Gestión de bloques/edificios
- Asignación a sucursal
- Ver aulas asociadas

#### Aulas (classrooms.html)
- Gestión de aulas/laboratorios
- Asignación a bloque
- Capacidad
- Ver computadoras asociadas

### 4. Computadoras (computers.html)

**Funciones:**
- Listar todas las computadoras
- Ver detalles (Nombre, SO, Modelo, Serial)
- Cambiar estado (Activa, Mantenimiento, Dada de baja)
- Desregistrar equipo
- Filtros avanzados:
  - Por sucursal
  - Por bloque
  - Por aula
  - Por estado en uso
  - Por estado de computadora

**Totalizadores:**
- Total Computadoras
- Activas
- En Mantenimiento
- En Uso

### 5. Sesiones (sessions.html)

**Funciones:**
- Listar todas las sesiones
- Filtros:
  - Por usuario o computadora
  - Por rango de fechas
- Ver duración de sesiones
- Distinguir activas/cerradas

**Totalizadores:**
- Total Sesiones
- Sesiones Activas
- Sesiones Cerradas
- Horas Totales

**Información mostrada:**
- Usuario
- Computadora
- Fecha/Hora inicio
- Fecha/Hora fin
- Duración
- Estado

### 6. Reportes (reports.html)

**Funciones:**
- Listar todos los reportes
- Ver detalles del problema
- Cambiar estado (Pendiente, En Revisión, Resuelto)
- Filtros:
  - Por estado
  - Por tipo de problema
  - Por fecha

**Información mostrada:**
- Fecha del reporte
- Usuario
- Computadora
- Tipo de problema
- Descripción
- Estado
- Fecha de resolución

### 7. Analíticas (analytics.html)

**Gráficos disponibles:**
- Uso por sucursal
- Uso por aula/laboratorio
- Sesiones activas por hora
- Reportes por tipo de problema
- Tendencias de uso

**Filtros:**
- Rango de fechas
- Por sucursal
- Por aula

### 8. Auditoría (audit.html)

**Funciones:**
- Visualizar logs de todas las operaciones
- Filtros:
  - Por usuario
  - Por acción
  - Por rango de fechas

**Información mostrada:**
- Usuario que realizó la acción
- Acción realizada
- Detalles
- Dirección IP
- Timestamp

## Características Técnicas

### Autenticación JWT

Todas las peticiones incluyen el token en el header:

```javascript
const authFetch = async (url, options = {}) => {
  const token = localStorage.getItem('authToken');
  return fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers
    }
  });
};
```

### Manejo de Sesiones

El token se almacena en localStorage:

```javascript
localStorage.setItem('authToken', data.token);
localStorage.setItem('userId', data.userId);
localStorage.setItem('userRole', data.roleName);
```

### Verificación de Autenticación

Cada página verifica autenticación al cargar:

```javascript
// En auth.js
if (!isAuthenticated()) {
  window.location.href = 'index.html';
}
```

### Cierre de Sesión

```javascript
function logout() {
  localStorage.removeItem('authToken');
  localStorage.removeItem('userId');
  localStorage.removeItem('userRole');
  window.location.href = 'index.html';
}
```

## Componentes Reutilizables

### Sidebar (sidebar.html)

Barra lateral de navegación común a todas las páginas:
- Logo y nombre del sistema
- Enlaces a módulos
- Indicador de página actual
- Botón de cerrar sesión

Se carga dinámicamente:

```javascript
fetch('sidebar.html')
  .then(r => r.text())
  .then(html => {
    document.getElementById('sidebar-placeholder').outerHTML = html;
  });
```

### Funciones de UI (ui.js)

Utilidades comunes:

```javascript
// Mostrar loading
showLoading('Cargando datos...');

// Ocultar loading
hideLoading();

// Mostrar toast de éxito/error
showToast('Operación exitosa', 'success');
showError('Error al guardar');

// Confirmación
showConfirm('¿Eliminar usuario?', () => {
  // Callback si confirma
});

// Inicializar UI
initUI('Sección', 'Página');
```

## Estilos

### Estructura CSS

```css
/* Variables personalizadas */
:root {
  --primary-color: #007bff;
  --success-color: #28a745;
  --danger-color: #dc3545;
  --warning-color: #ffc107;
}

/* Sidebar */
#sidebar {
  width: 250px;
  min-height: 100vh;
  background: #343a40;
}

/* Cards de KPIs */
.card {
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}
```

### Responsive Design

El dashboard es completamente responsive:
- Desktop: Sidebar fijo, layout de 3 columnas
- Tablet: Sidebar colapsable, layout de 2 columnas
- Mobile: Sidebar oculto, layout de 1 columna

## Gráficos (Chart.js)

### Ejemplo de implementación

```javascript
const ctx = document.getElementById('usageChart').getContext('2d');
new Chart(ctx, {
  type: 'bar',
  data: {
    labels: ['America', 'Tiquipaya', 'Ayacucho'],
    datasets: [{
      label: 'Horas de Uso',
      data: [120, 95, 80],
      backgroundColor: 'rgba(54, 162, 235, 0.6)'
    }]
  },
  options: {
    responsive: true,
    maintainAspectRatio: false
  }
});
```

## Manejo de Errores

### Errores de API

```javascript
try {
  const response = await authFetch('/api/users');
  if (!response.ok) {
    throw new Error('Error al cargar usuarios');
  }
  const data = await response.json();
} catch (err) {
  showError('Error al comunicarse con el servidor');
  console.error(err);
}
```

### Token Expirado

```javascript
if (response.status === 401) {
  localStorage.clear();
  window.location.href = 'index.html';
}
```

## Seguridad

### Protección de Rutas

Todas las páginas (excepto index.html) verifican autenticación:

```javascript
// Al inicio de cada página
if (!localStorage.getItem('authToken')) {
  window.location.href = 'index.html';
}
```

### XSS Prevention

Se usa `textContent` en lugar de `innerHTML` para datos dinámicos:

```javascript
element.textContent = userInput; // Seguro
element.innerHTML = userInput;   // Inseguro
```

### CSRF Protection

Las peticiones POST/PUT/DELETE incluyen validación de token JWT.

## Navegadores Soportados

- Chrome 90+
- Firefox 88+
- Edge 90+
- Safari 14+

## Solución de Problemas

### No se conecta a la API
- Verificar que la API esté ejecutándose
- Confirmar URL correcta en `js/api.js`
- Verificar CORS en la API

### Token expirado constantemente
- Aumentar `ExpirationMinutes` en `appsettings.json` de la API
- Verificar sincronización de fecha/hora del sistema

### Gráficos no se muestran
- Verificar que Chart.js esté cargado
- Abrir consola del navegador para ver errores
- Confirmar que los datos del backend sean correctos

### Sidebar no aparece
- Verificar ruta de `sidebar.html`
- Revisar consola del navegador
- Confirmar que servidor web esté sirviendo archivos correctamente

## Mejoras Futuras

- Notificaciones en tiempo real con WebSockets
- Exportación de reportes a PDF/Excel
- Temas claro/oscuro
- Idioma multi-lenguaje
- PWA (Progressive Web App)
- Caché de datos con Service Workers

## Contacto

Para soporte o consultas:
- Email: ghr0034560@est.univalle.edu
- Autor: Rodrigo Gutierrez Herrera

---

**Versión:** 1.0.0  
**Última actualización:** Diciembre 2025
