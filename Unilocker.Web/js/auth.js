const API_BASE_URL = "http://localhost:7198"; // ajusta si usas otra URL

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
 * Paso 1: intenta login con usuario/contraseña.
 * Si la API responde { requiresVerification: true, userId } no se guarda token todavía.
 * Si responde { token, ... } se guarda token y se redirige.
 */
export async function startLogin(username, password) {
  const resp = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password })
  });

  if (!resp.ok) {
    throw new Error("Usuario o contraseña incorrectos");
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
  const resp = await fetch(`${API_BASE_URL}/api/auth/verify-code`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, code })
  });

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
  const resp = await fetch(`${API_BASE_URL}/api/auth/resend-code`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId })
  });

  if (!resp.ok) {
    throw new Error("No se pudo reenviar el código");
  }

  // Si quieres, puedes leer un mensaje del backend:
  // const data = await resp.json();
  return;
}
