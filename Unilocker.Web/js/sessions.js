import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let sessionsCache = [];

function formatDateTime(isoString) {
    if (!isoString) return '-';
    const d = new Date(isoString);
    if (isNaN(d)) return '-';
    return d.toLocaleString();
}

function renderSessions(items) {
    const tbody = document.getElementById('sessionsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">No se encontraron sesiones</td></tr>';
        return;
    }

    items.forEach(s => {
        const statusBadge = s.isActive
            ? '<span class="badge bg-success">Activa</span>'
            : '<span class="badge bg-secondary">Cerrada</span>';

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${s.userName ?? '-'}</td>
            <td>${s.computerName ?? '-'}</td>
            <td>${formatDateTime(s.startTime)}</td>
            <td>${formatDateTime(s.endTime)}</td>
            <td>${statusBadge}</td>
        `;
        tbody.appendChild(tr);
    });
}

export async function loadRecords(username = '') {
    showLoading('Cargando sesiones...');
    try {
        let url = '/api/sessions';
        if (username && username.trim() !== '') {
            url += `?username=${encodeURIComponent(username.trim())}`;
        }

        const res = await authFetch(url);
        if (!res.ok) {
            showError('Error cargando sesiones.');
            return;
        }
        const data = await res.json();
        sessionsCache = data;
        renderSessions(sessionsCache);
    } catch (err) {
        console.error(err);
        showError(err.message || 'Error al cargar sesiones');
    } finally {
        hideLoading();
    }
}

// Ejemplo de acción sobre sesiones (si tienes endpoint para cerrar sesión desde el admin)
export async function closeSession(id) {
    return authFetch(`/api/sessions/${id}/close`, {
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
    const btnFilter = document.getElementById('btnFilterSessions');
    const btnClear = document.getElementById('btnClearFilter');
    const filterInput = document.getElementById('filterUsername');

    if (btnFilter) {
        btnFilter.addEventListener('click', async () => {
            const username = filterInput?.value || '';
            await loadRecords(username);
        });
    }

    if (btnClear) {
        btnClear.addEventListener('click', async () => {
            if (filterInput) filterInput.value = '';
            await loadRecords('');
        });
    }

    if (filterInput) {
        filterInput.addEventListener('keypress', async (e) => {
            if (e.key === 'Enter') {
                await loadRecords(filterInput.value || '');
            }
        });
    }

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
