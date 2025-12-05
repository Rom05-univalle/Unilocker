import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let sessionsCache = [];
let usersOptions = [];

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

function applyFilter() {
    const filterUser = document.getElementById('filterUser');
    const userId = filterUser ? parseInt(filterUser.value || '0', 10) : 0;

    let filtered = [...sessionsCache];
    if (userId > 0) {
        filtered = filtered.filter(s => s.userId === userId);
    }
    renderSessions(filtered);
}

async function loadUsers() {
    try {
        const resp = await authFetch('/api/users');
        const data = await resp.json();

        usersOptions = data.map(u => ({
            id: u.id,
            username: u.username,
            fullName: `${u.firstName || ''} ${u.lastName || ''}`.trim()
        }));

        populateUserFilter();
    } catch (err) {
        console.error(err);
    }
}

function populateUserFilter() {
    const filterUser = document.getElementById('filterUser');
    if (!filterUser) return;

    filterUser.innerHTML = '<option value="">Todos los usuarios</option>';
    usersOptions.forEach(u => {
        const opt = document.createElement('option');
        opt.value = u.id;
        opt.textContent = `${u.username} - ${u.fullName || u.username}`;
        filterUser.appendChild(opt);
    });
}

export async function loadRecords() {
    showLoading('Cargando sesiones...');
    try {
        const res = await authFetch('/api/sessions');
        if (!res.ok) {
            showError('Error cargando sesiones.');
            return;
        }
        const data = await res.json();
        sessionsCache = data;
        applyFilter();
    } catch (err) {
        console.error(err);
        showError(err);
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
    const filterUser = document.getElementById('filterUser');
    if (filterUser) {
        filterUser.addEventListener('change', applyFilter);
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
    await loadUsers();
    await loadRecords();
}

document.addEventListener('DOMContentLoaded', init);
