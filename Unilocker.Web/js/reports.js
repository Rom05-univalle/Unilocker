import { API_BASE_URL, authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError } from './ui.js';

let reportsCache = [];

function renderReports(items) {
    const tbody = document.getElementById('reportsTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    items.forEach(r => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${r.id}</td>
            <td>${r.date ?? '-'}</td>
            <td>${r.userName ?? '-'}</td>
            <td>${r.computerName ?? '-'}</td>
            <td>${r.problemTypeName ?? '-'}</td>
            <td>${r.summary ?? '-'}</td>
        `;
        tbody.appendChild(tr);
    });
}

function getFilters() {
    const startInput = document.getElementById('reportStartDate');
    const endInput = document.getElementById('reportEndDate');
    const startDate = startInput?.value || null;
    const endDate = endInput?.value || null;
    return { startDate, endDate };
}

export async function loadReports() {
    const { startDate, endDate } = getFilters();

    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const url = params.toString()
        ? `${API_BASE_URL}/api/reports?${params.toString()}`
        : `${API_BASE_URL}/api/reports`;

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
    if (startInput) startInput.value = '';
    if (endInput) endInput.value = '';
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
            const idCell = tr.firstElementChild;
            const id = idCell?.textContent?.trim();
            if (!id) return;
            // Por ejemplo, clic en cualquier fila abre detalle
            window.location.href = `report-detail.html?id=${encodeURIComponent(id)}`;
        });
    }
}

async function init() {
    setupEvents();
    await loadReports();
}

document.addEventListener('DOMContentLoaded', init);
