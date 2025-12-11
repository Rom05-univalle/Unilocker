// Para red local: cambiar localhost por la IP del servidor (ej: "http://192.168.0.5:5013")
const API_BASE_URL = "http://192.168.0.7:5013"; // puerto de tu Unilocker.Api

export function setToken(token) {
  localStorage.setItem("jwt", token);
}
export function getToken() {
  return localStorage.getItem("jwt");
}
export function removeToken() {
  localStorage.removeItem("jwt");
}
export function isAuthenticated() {
  return !!getToken();
}
export function logout() {
  removeToken();
  window.location.href = "login.html";
}

/**
 * Decodifica el JWT y retorna los datos del usuario actual
 * @returns {object|null} { userId, roleId, roleName, username, fullName } o null si no hay token
 */
export function getCurrentUser() {
  const token = getToken();
  if (!token) return null;

  try {
    // Decodificar la parte del payload (parte 2 del JWT)
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const payload = JSON.parse(atob(parts[1]));
    
    return {
      userId: parseInt(payload.userId || 0, 10),
      roleId: parseInt(payload.roleId || 0, 10),
      roleName: payload.roleName || '',
      username: payload.sub || '',
      fullName: payload.fullName || ''
    };
  } catch (err) {
    console.error('Error decodificando JWT:', err);
    return null;
  }
}

/**
 * Paso 1: intenta login con usuario/contraseña.
 * Si la API responde { requiresVerification: true, userId } no se guarda token todavía.
 * Si responde { token, ... } se guarda token y se redirige.
 */
export async function startLogin(username, password) {
  let resp;
  
  try {
    resp = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password })
    });
  } catch (fetchError) {
    // Error de conexión: servidor no accesible
    const connectionError = new Error(
      `No se pudo conectar con el servidor.\nVerifica que la API esté corriendo en: ${API_BASE_URL}`
    );
    connectionError.isConnectionError = true;
    throw connectionError;
  }

  if (!resp.ok) {
    // Intentar leer el mensaje específico del backend
    let errorMessage = "Usuario o contraseña incorrectos";
    try {
      const errorData = await resp.json();
      if (errorData.message) {
        errorMessage = errorData.message;
      }
    } catch (e) {
      // Si no se puede parsear el JSON, usar el mensaje por defecto
    }
    
    // Diferenciar entre errores 401 (credenciales incorrectas) y 403 (usuario bloqueado/inactivo)
    const error = new Error(errorMessage);
    error.status = resp.status;
    throw error;
  }

  const data = await resp.json();

  // Caso 2FA requerido
  if (data.requiresVerification) {
    // aquí asumes que la API ya envió el código por email y devuelve userId
    if (!data.userId) {
      throw new Error("Falta userId para verificación 2FA");
    }
    return {
      requiresVerification: true,
      userId: data.userId
    };
  }

  // Caso login normal (sin 2FA)
  if (!data.token) {
    throw new Error("Respuesta de autenticación inválida");
  }

  setToken(data.token);
  window.location.href = "dashboard.html";
  return { requiresVerification: false };
}

/**
 * Paso 2: verificar código 2FA
 * Espera que la API /api/auth/verify-code reciba { userId, code } y devuelva { token }
 */
export async function verifyCode(userId, code) {
  let resp;
  
  try {
    resp = await fetch(`${API_BASE_URL}/api/auth/verify-code`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userId, code })
    });
  } catch (fetchError) {
    // Error de conexión
    const connectionError = new Error(
      `No se pudo conectar con el servidor.\nVerifica que la API esté corriendo en: ${API_BASE_URL}`
    );
    connectionError.isConnectionError = true;
    throw connectionError;
  }

  if (!resp.ok) {
    if (resp.status === 400 || resp.status === 401) {
      // Código incorrecto o expirado
      const errText = await resp.text().catch(() => "");
      throw new Error(errText || "Código inválido o expirado");
    }
    throw new Error(`Error al verificar código (${resp.status})`);
  }

  const data = await resp.json();
  if (!data.token) {
    throw new Error("Respuesta inválida al verificar código");
  }

  setToken(data.token);
  window.location.href = "dashboard.html";
}

/**
 * Reenviar código: puedes decidir si llamas a un endpoint específico
 * o reutilizas /api/auth/login con el mismo usuario.
 * Aquí se asume un endpoint /api/auth/resend-code con { userId }.
 */
export async function resendCode(userId) {
  let resp;
  
  try {
    resp = await fetch(`${API_BASE_URL}/api/auth/resend-code`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userId })
    });
  } catch (fetchError) {
    // Error de conexión
    const connectionError = new Error(
      `No se pudo conectar con el servidor.\nVerifica que la API esté corriendo en: ${API_BASE_URL}`
    );
    connectionError.isConnectionError = true;
    throw connectionError;
  }

  if (!resp.ok) {
    throw new Error("No se pudo reenviar el código");
  }

  // Si quieres, puedes leer un mensaje del backend:
  // const data = await resp.json();
  return;
}
