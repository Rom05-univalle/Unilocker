import { getToken, logout } from './auth.js';

// Para red local: cambiar localhost por la IP del servidor (ej: "http://192.168.0.5:5013")
export const API_BASE_URL = "http://localhost:5000";

export async function authFetch(relativeEndpoint, options = {}) {
  const endpoint = `${API_BASE_URL}${relativeEndpoint}`;

  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
  };

  const token = getToken();
  if (token) {
    headers["Authorization"] = "Bearer " + token;
  }

  const resp = await fetch(endpoint, {
    method: options.method || "GET",
    headers,
    body: options.body
      ? (typeof options.body === "string" ? options.body : JSON.stringify(options.body))
      : undefined,
  });

  // 1) Si NO hay token y el backend responde 401 → sesión vencida, redirigir al login.
  if (resp.status === 401 && !token) {
    window.showToast("Sesión vencida. Inicia sesión nuevamente.", "error");
    setTimeout(logout, 1500);
    throw new Error("No autenticado");
  }

  // 2) Si HAY token y el backend responde 401 → no rompemos sesión, solo devolvemos resp.
  if (resp.status === 401 && token) {
    window.showToast("No autorizado para esta operación.", "error");
    return resp;
  }

  // 3) Otros errores - extraer mensaje del servidor si existe
  if (!resp.ok) {
    let errorMsg = `Error API (${resp.status})`;
    try {
      const errorData = await resp.json();
      if (errorData.message) {
        errorMsg = errorData.message;
      }
    } catch (e) {
      // Si no se puede parsear JSON, usar mensaje genérico
    }
    // NO mostrar toast aquí, dejar que el código llamador lo maneje
    const error = new Error(errorMsg);
    error.response = resp;
    throw error;
  }

  return resp;
}

// Función de ayuda para toasts simples (fallback)
window.showToast = function (msg, type = "info") {
  let toast = document.createElement("div");
  toast.className = `toast align-items-center text-bg-${type} position-fixed bottom-0 end-0 m-3`;
  toast.role = "alert";
  toast.innerHTML = `
    <div class="d-flex">
      <div class="toast-body">${msg}</div>
      <button type="button" class="btn-close btn-close-white me-2 m-auto"
              data-bs-dismiss="toast" aria-label="Cerrar"></button>
    </div>
  `;
  document.body.appendChild(toast);
  const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
  bsToast.show();
  bsToast._element.addEventListener("hidden.bs.toast", () => toast.remove());
};
