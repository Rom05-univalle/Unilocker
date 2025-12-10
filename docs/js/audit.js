import { authFetch } from './api.js';
import { showLoading, hideLoading, showError } from './ui.js';

let currentPage = 1;
const pageSize = 20;

function getFilters() {
    const table = document.getElementById('filterTable')?.value.trim() || '';
    const actionType = document.getElementById('filterAction')?.value.trim() || '';
    const user = document.getElementById('filterUser')?.value.trim() || '';
    const from = document.getElementById('filterFrom')?.value || '';
    const to = document.getElementById('filterTo')?.value || '';

    return { table, actionType, user, from, to };
}

function formatDateTime(isoString) {
    if (!isoString) return '';
    const d = new Date(isoString);
    if (isNaN(d)) return '';
    return d.toLocaleString();
}

function renderTable(items) {
    const tbody = document.getElementById('auditTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (!items || items.length === 0) {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td colspan="8" class="text-center text-muted py-3">
                No se encontraron registros de auditoría.
            </td>
        `;
        tbody.appendChild(tr);
        return;
    }

    items.forEach(a => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${formatDateTime(a.actionDate)}</td>
            <td>${a.responsibleUserName || ''}</td>
            <td>${a.actionType || ''}</td>
            <td>${a.affectedTable || ''}</td>
            <td>${a.recordId}</td>
            <td class="text-center">
                <button class="btn btn-sm btn-outline-info btn-view-details" data-details='${JSON.stringify(a.changeDetails || '')}' data-user="${a.responsibleUserName || ''}" data-action="${a.actionType || ''}" data-table="${a.affectedTable || ''}">
                    <i class="fa-solid fa-eye"></i> Ver detalles
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function renderPagination(total, page, pageSize) {
    const container = document.getElementById('auditPagination');
    const info = document.getElementById('auditSummary');
    if (!container || !info) return;

    container.innerHTML = '';

    const totalPages = Math.max(1, Math.ceil(total / pageSize));
    info.textContent = `Mostrando página ${page} de ${totalPages} (${total} registros)`;

    if (totalPages <= 1) return;

    const createBtn = (p, text, disabled = false, active = false) => {
        const li = document.createElement('li');
        li.className = `page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}`;
        const a = document.createElement('button');
        a.className = 'page-link';
        a.textContent = text;
        a.type = 'button';
        a.addEventListener('click', () => {
            if (p !== currentPage && !disabled) {
                currentPage = p;
                loadAudit();
            }
        });
        li.appendChild(a);
        return li;
    };

    container.appendChild(createBtn(page - 1, '«', page === 1, false));

    for (let p = 1; p <= totalPages; p++) {
        if (p === 1 || p === totalPages || Math.abs(p - page) <= 2) {
            container.appendChild(createBtn(p, p.toString(), false, p === page));
        } else if (Math.abs(p - page) === 3) {
            const li = document.createElement('li');
            li.className = 'page-item disabled';
            li.innerHTML = `<span class="page-link">…</span>`;
            container.appendChild(li);
        }
    }

    container.appendChild(createBtn(page + 1, '»', page === totalPages, false));
}

async function loadAudit() {
    showLoading('Cargando auditoría...');
    try {
        const { table, actionType, user, from, to } = getFilters();

        const params = new URLSearchParams();
        if (table) params.append('table', table);
        if (actionType) params.append('actionType', actionType);
        if (user) params.append('user', user);
        if (from) params.append('from', from);
        if (to) params.append('to', to);
        params.append('page', currentPage);
        params.append('pageSize', pageSize);

        const resp = await authFetch(`/api/audit?${params.toString()}`);
        if (!resp.ok) {
            throw new Error(`Error al cargar auditoría (${resp.status})`);
        }
        const data = await resp.json();

        renderTable(data.items || []);
        renderPagination(data.total || 0, data.page || currentPage, data.pageSize || pageSize);
    } catch (err) {
        console.error(err);
        showError('No se pudo cargar la auditoría.');
    } finally {
        hideLoading();
    }
}

function initFilters() {
    const form = document.getElementById('auditFilterForm');
    if (form) {
        form.addEventListener('submit', e => {
            e.preventDefault();
            currentPage = 1;
            loadAudit();
        });
    }

    const btnClear = document.getElementById('btnAuditClear');
    if (btnClear) {
        btnClear.addEventListener('click', () => {
            const ids = ['filterTable', 'filterAction', 'filterUser', 'filterFrom', 'filterTo'];
            ids.forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = '';
            });
            currentPage = 1;
            loadAudit();
        });
    }

    // Event listener para botones "Ver detalles"
    const tbody = document.getElementById('auditTableBody');
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const btn = e.target.closest('.btn-view-details');
            if (btn) {
                const details = btn.dataset.details;
                const user = btn.dataset.user;
                const action = btn.dataset.action;
                const table = btn.dataset.table;
                
                // Formatear JSON si es posible
                let formattedDetails = details;
                try {
                    const parsed = JSON.parse(details);
                    formattedDetails = JSON.stringify(parsed, null, 2);
                } catch (err) {
                    // Si no es JSON válido, mostrar como está
                    formattedDetails = details;
                }
                
                // Llenar el modal
                document.getElementById('detailUser').textContent = user;
                document.getElementById('detailAction').textContent = action;
                document.getElementById('detailTable').textContent = table;
                document.getElementById('detailChanges').textContent = formattedDetails;
                
                // Mostrar modal
                const modal = new bootstrap.Modal(document.getElementById('auditDetailsModal'));
                modal.show();
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    initFilters();
    loadAudit();
});
