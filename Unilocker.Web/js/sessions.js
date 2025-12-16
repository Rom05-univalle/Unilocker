import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let sessionsCache = [];

function formatDateTime(isoString) {
    if (!isoString) return '-';
    const d = new Date(isoString);
    if (isNaN(d)) return '-';
    return d.toLocaleString();
}

function formatDuration(minutes) {
    if (minutes === null || minutes === undefined || minutes < 0) return '-';
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    if (hours > 0) {
        return `${hours}h ${mins}m`;
    }
    return `${mins}m`;
}

function renderSessions(items) {
    const tbody = document.getElementById('sessionsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-3">No se encontraron sesiones</td></tr>';
        return;
    }

    items.forEach(s => {
        const statusBadge = s.isActive
            ? '<span class="badge bg-success">Activa</span>'
            : '<span class="badge bg-secondary">Cerrada</span>';

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${s.userFullName ?? s.userName ?? '-'}</td>
            <td>${s.computerName ?? '-'}</td>
            <td>${formatDateTime(s.startTime)}</td>
            <td>${formatDateTime(s.endTime)}</td>
            <td>${formatDuration(s.durationMinutes)}</td>
            <td>${statusBadge}</td>
        `;
        tbody.appendChild(tr);
    });
}

function updateTotalizers(sessions) {
    const total = sessions.length;
    const active = sessions.filter(s => s.isActive).length;
    const closed = total - active;
    const totalHours = sessions.reduce((sum, s) => sum + (s.durationMinutes || 0), 0) / 60;

    document.getElementById('totalSessionsCount').innerText = total;
    document.getElementById('activeSessionsCount').innerText = active;
    document.getElementById('closedSessionsCount').innerText = closed;
    document.getElementById('totalHoursCount').innerText = Math.round(totalHours) + 'h';
}

export async function loadRecords(username = '', startDate = '', endDate = '') {
    showLoading('Cargando sesiones...');
    try {
        let url = '/api/sessions?pageSize=500'; // Aumentar límite para totalizadores
        if (username && username.trim() !== '') {
            url += `&username=${encodeURIComponent(username.trim())}`;
        }
        if (startDate) {
            url += `&startDate=${encodeURIComponent(startDate)}`;
        }
        if (endDate) {
            url += `&endDate=${encodeURIComponent(endDate)}`;
        }

        const res = await authFetch(url);
        if (!res.ok) {
            showError('Error cargando sesiones.');
            return;
        }
        const data = await res.json();
        sessionsCache = data;
        renderSessions(sessionsCache);
        updateTotalizers(sessionsCache);
    } catch (err) {
        window.handleApiError(err, err.message || 'Error al cargar sesiones');
    } finally {
        hideLoading();
    }
}

// Ejemplo de acci�n sobre sesiones (si tienes endpoint para cerrar sesi�n desde el admin)
export async function closeSession(id) {
    return authFetch(`/api/sessions/${id}/close`, {
        method: 'PUT'
    });
}

function askCloseSession(id) {
    showConfirm('�Seguro que quieres cerrar esta sesi�n?', async () => {
        showLoading('Cerrando sesi�n...');
        try {
            const res = await closeSession(id);
            if (!res.ok) {
                showError('No se pudo cerrar la sesi�n.');
                return;
            }
            await loadRecords();
            showToast('Sesi�n cerrada correctamente', 'success');
        } catch (err) {
            window.handleApiError(err, err);
        } finally {
            hideLoading();
        }
    });
}

function setupEvents() {
    const btnFilter = document.getElementById('btnFilterSessions');
    const btnClear = document.getElementById('btnClearFilter');
    const filterInput = document.getElementById('filterUsername');
    const filterStartDate = document.getElementById('filterStartDate');
    const filterEndDate = document.getElementById('filterEndDate');

    if (btnFilter) {
        btnFilter.addEventListener('click', async () => {
            const username = filterInput?.value || '';
            const startDate = filterStartDate?.value || '';
            const endDate = filterEndDate?.value || '';
            await loadRecords(username, startDate, endDate);
        });
    }

    if (btnClear) {
        btnClear.addEventListener('click', async () => {
            if (filterInput) filterInput.value = '';
            if (filterStartDate) filterStartDate.value = '';
            if (filterEndDate) filterEndDate.value = '';
            await loadRecords('', '', '');
        });
    }

    if (filterInput) {
        filterInput.addEventListener('keypress', async (e) => {
            if (e.key === 'Enter') {
                const startDate = filterStartDate?.value || '';
                const endDate = filterEndDate?.value || '';
                await loadRecords(filterInput.value || '', startDate, endDate);
            }
        });
    }

    const tbody = document.getElementById('sessionsTableBody');
    if (!tbody) return;

    // Solo si agregas botones de acci�n en la tabla de sesiones
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
