import { API_BASE_URL, authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let sessionsCache = [];

function renderSessions(items) {
    const tbody = document.getElementById('sessionsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    items.forEach(s => {
        const statusBadge = s.isActive
            ? '<span class="badge bg-success">Activa</span>'
            : '<span class="badge bg-secondary">Cerrada</span>';

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${s.id}</td>
            <td>${s.userName ?? '-'}</td>
            <td>${s.computerName ?? '-'}</td>
            <td>${s.startTime ?? '-'}</td>
            <td>${s.endTime ?? '-'}</td>
            <td>${statusBadge}</td>
        `;
        tbody.appendChild(tr);
    });
}

export async function loadRecords() {
    showLoading('Cargando sesiones...');
    try {
        const res = await authFetch(`${API_BASE_URL}/api/sessions`);
        if (!res.ok) {
            showError('Error cargando sesiones.');
            return;
        }
        const data = await res.json();
        sessionsCache = data;
        renderSessions(data);
    } catch (err) {
        console.error(err);
        showError(err);
    } finally {
        hideLoading();
    }
}

// Ejemplo de acción sobre sesiones (si tienes endpoint para cerrar sesión desde el admin)
export async function closeSession(id) {
    return authFetch(`${API_BASE_URL}/api/sessions/${id}/close`, {
        method: 'PUT'
    });
}

function askCloseSession(id) {
    showConfirm('¿Seguro que quieres cerrar esta sesión?', async () => {
        showLoading('Cerrando sesión...');
        try {
            const res = await closeSession(id);
            if (!res.ok) {
                showError('No se pudo cerrar la sesión.');
                return;
            }
            await loadRecords();
            showToast('Sesión cerrada correctamente', 'success');
        } catch (err) {
            console.error(err);
            showError(err);
        } finally {
            hideLoading();
        }
    });
}

function setupEvents() {
    const tbody = document.getElementById('sessionsTableBody');
    if (!tbody) return;

    // Solo si agregas botones de acción en la tabla de sesiones
    tbody.addEventListener('click', (e) => {
        const btn = e.target.closest('button');
        if (!btn) return;
        const action = btn.dataset.action;
        const id = btn.dataset.id;
        if (action === 'close') {
            askCloseSession(id);
        }
    });
}

async function init() {
    setupEvents();
    await loadRecords();
}

document.addEventListener('DOMContentLoaded', init);
