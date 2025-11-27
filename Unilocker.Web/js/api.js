import { getToken, logout } from './auth.js';

const API_BASE_URL = "https://localhost:7198";

export async function apiCall(relativeEndpoint, method = "GET", body = null) {
  const endpoint = `${API_BASE_URL}${relativeEndpoint}`;

  const opts = {
    method,
    headers: {
      "Content-Type": "application/json",
      "Authorization": "Bearer " + getToken()
    }
  };
  if (body) opts.body = JSON.stringify(body);

  const resp = await fetch(endpoint, opts);

  if (resp.status === 401) {
    showToast("Sesión vencida. Inicia sesión nuevamente.", "danger");
    setTimeout(logout, 1500);
    throw new Error("No autenticado");
  }

  if (!resp.ok) {
    const errMsg = `Error API (${resp.status})`;
    showToast(errMsg, "danger");
    throw new Error(errMsg);
  }

  return await resp.json();
}

window.showToast = function(msg, type = "info") {
  let toast = document.createElement("div");
  toast.className = `toast align-items-center text-bg-${type} position-fixed bottom-0 end-0 m-3`;
  toast.role = "alert";
  toast.innerHTML = `
      <div class="d-flex"><div class="toast-body">${msg}</div>
      <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>
  `;
  document.body.appendChild(toast);
  let bsToast = new bootstrap.Toast(toast, { delay: 3000 });
  bsToast.show();
  toast.addEventListener('hidden.bs.toast', () => { toast.remove(); });
};
