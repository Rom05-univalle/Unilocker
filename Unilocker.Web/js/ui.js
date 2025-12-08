// js/ui.js
let toastInstance = null;
let confirmModalInstance = null;
let confirmCallback = null;

// Inicialización común de UI (llamar en cada página)
export function initUI(sectionText = null, currentText = null) {
    // Breadcrumb
    const section = document.getElementById('breadcrumbSection');
    const current = document.getElementById('breadcrumbCurrent');
    if (section && sectionText) section.textContent = sectionText;
    if (current && currentText) current.textContent = currentText;

    // Toast
    const toastEl = document.getElementById('mainToast');
    if (toastEl && window.bootstrap) {
        toastInstance = new bootstrap.Toast(toastEl, { delay: 3000 });
    }

    // Modal de confirmación global (opcional)
    const confirmEl = document.getElementById('globalConfirmModal');
    if (confirmEl && window.bootstrap) {
        confirmModalInstance = new bootstrap.Modal(confirmEl);
        const btnOk = document.getElementById('globalConfirmOk');
        const btnCancel = document.getElementById('globalConfirmCancel');

        if (btnOk) {
            btnOk.onclick = () => {
                if (confirmCallback) confirmCallback(true);
                confirmCallback = null;
                confirmModalInstance.hide();
            };
        }

        if (btnCancel) {
            btnCancel.onclick = () => {
                if (confirmCallback) confirmCallback(false);
                confirmCallback = null;
                confirmModalInstance.hide();
            };
        }
    }

    // Logout
    const logoutLink = document.getElementById('logoutLink') || document.getElementById('logoutBtn');
    if (logoutLink) {
        logoutLink.addEventListener('click', (e) => {
            e.preventDefault();
            localStorage.removeItem('jwt');
            window.location.href = 'login.html';
        });
    }
}

// Overlay de carga global
export function showLoading(message = 'Cargando...') {
    const overlay = document.getElementById('globalLoader');
    if (!overlay) return;
    const span = overlay.querySelector('.loader-message');
    if (span) span.textContent = message;
    overlay.classList.remove('d-none');
}

export function hideLoading() {
    const overlay = document.getElementById('globalLoader');
    if (!overlay) return;
    overlay.classList.add('d-none');
}

// Toasts
export function showToast(message, type = 'success') {
    const toastEl = document.getElementById('mainToast');
    const toastBody = document.getElementById('mainToastBody');
    if (!toastEl || !toastBody || !toastInstance) return;

    toastBody.textContent = message;
    toastEl.className = 'toast align-items-center border-0 text-bg-primary';

    if (type === 'error') {
        toastEl.classList.replace('text-bg-primary', 'text-bg-danger');
    } else if (type === 'warning') {
        toastEl.classList.replace('text-bg-primary', 'text-bg-warning');
    } else if (type === 'info') {
        toastEl.classList.replace('text-bg-primary', 'text-bg-info');
    } else {
        toastEl.classList.replace('text-bg-primary', 'text-bg-success');
    }

    toastInstance.show();
}

// Confirmación reutilizable: devuelve Promise<boolean>
// Acepta title y message para mostrar HTML en el cuerpo
export function showConfirm(titleOrMessage, message = null) {
    const titleEl = document.getElementById('globalConfirmTitle');
    const msgEl = document.getElementById('globalConfirmMessage');

    return new Promise((resolve) => {
        // Si no hay modal global, usar window.confirm con solo texto
        if (!confirmModalInstance || !msgEl) {
            const textOnly = message ? `${titleOrMessage}\n${message}` : titleOrMessage;
            const ok = window.confirm(textOnly);
            resolve(ok);
            return;
        }

        // Si se pasa un segundo parámetro, el primero es el título
        if (message) {
            if (titleEl) titleEl.textContent = titleOrMessage;
            msgEl.innerHTML = message; // Permitir HTML en el mensaje
        } else {
            if (titleEl) titleEl.textContent = 'Confirmar acción';
            msgEl.innerHTML = titleOrMessage; // Permitir HTML
        }

        confirmCallback = (accepted) => {
            resolve(accepted);
        };
        confirmModalInstance.show();
    });
}

// Manejo de errores
export function showError(error) {
    let message = 'Ocurrió un error inesperado.';
    if (typeof error === 'string') {
        message = error;
    } else if (error && error.message) {
        message = error.message;
    }
    showToast(message, 'error');
}
