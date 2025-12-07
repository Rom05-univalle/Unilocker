import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError } from './ui.js';

let reportsCache = [];

function formatDateTime(isoString) {
    if (!isoString) return '-';
    const d = new Date(isoString);
    if (isNaN(d)) return '-';
    return d.toLocaleString();
}

function getStatusBadge(status) {
    const badges = {
        'Pending': '<span class="badge bg-warning text-dark">Pendiente</span>',
        'InReview': '<span class="badge bg-info">En Revisión</span>',
        'Resolved': '<span class="badge bg-success">Resuelto</span>',
        'Rejected': '<span class="badge bg-danger">Rechazado</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

function renderReports(items) {
    const tbody = document.getElementById('reportsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    items.forEach(r => {
        const tr = document.createElement('tr');
        tr.dataset.reportId = r.id; // Guardar el ID en el data attribute
        tr.style.cursor = 'pointer';
        tr.innerHTML = `
            <td>${formatDateTime(r.reportDate)}</td>
            <td>${r.userName ?? '-'}</td>
            <td>${r.computerName ?? '-'}</td>
            <td>${r.problemTypeName ?? '-'}</td>
            <td class="text-center">
                <button class="btn btn-sm btn-outline-info" style="pointer-events: none;">
                    <i class="fa-solid fa-eye"></i> Ver detalles
                </button>
            </td>
            <td>${getStatusBadge(r.reportStatus)}</td>
        `;
        tbody.appendChild(tr);
    });
}

function getFilters() {
    const startInput = document.getElementById('reportStartDate');
    const endInput = document.getElementById('reportEndDate');
    const statusInput = document.getElementById('reportStatusFilter');
    const startDate = startInput?.value || null;
    const endDate = endInput?.value || null;
    const status = statusInput?.value || null;
    return { startDate, endDate, status };
}

export async function loadReports() {
    const { startDate, endDate, status } = getFilters();

    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (status) params.append('status', status);

    const url = params.toString()
        ? `/api/reports?${params.toString()}`
        : `/api/reports`;

    showLoading('Cargando reportes...');
    try {
        const res = await authFetch(url);
        if (!res.ok) {
            showError('Error cargando reportes.');
            return;
        }
        const data = await res.json();
        reportsCache = data;
        renderReports(data);
    } catch (err) {
        console.error(err);
        showError(err);
    } finally {
        hideLoading();
    }
}

function clearFilters() {
    const startInput = document.getElementById('reportStartDate');
    const endInput = document.getElementById('reportEndDate');
    const statusInput = document.getElementById('reportStatusFilter');
    if (startInput) startInput.value = '';
    if (endInput) endInput.value = '';
    if (statusInput) statusInput.value = '';
}

function setupEvents() {
    const btnFilter = document.getElementById('btnFilterReports');
    const btnClear = document.getElementById('btnClearReports');
    const tbody = document.getElementById('reportsTableBody');

    if (btnFilter) {
        btnFilter.addEventListener('click', async () => {
            await loadReports();
        });
    }

    if (btnClear) {
        btnClear.addEventListener('click', async () => {
            clearFilters();
            await loadReports();
        });
    }

    // Navegar a detalle si tienes report-detail.html
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const tr = e.target.closest('tr');
            if (!tr) return;
            const reportId = tr.dataset.reportId; // Obtener el ID del data attribute
            if (!reportId) return;
            window.location.href = `report-detail.html?id=${reportId}`;
        });
    }
}

async function init() {
    setupEvents();
    await loadReports();
}

document.addEventListener('DOMContentLoaded', init);
