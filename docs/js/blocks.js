import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let blockModal;
let blocksCache = [];
let branchesCache = [];

function renderBlocks(rows) {
    const tbody = document.getElementById('blocksTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    rows.forEach(block => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${block.name}</td>
            <td>${block.branchName ?? ''}</td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${block.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${block.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function applyFilter() {
    const search = document.getElementById('txtSearch')?.value?.toLowerCase() || '';
    const filtered = blocksCache.filter(b =>
        b.name.toLowerCase().includes(search) ||
        (b.branchName ?? '').toLowerCase().includes(search)
    );
    renderBlocks(filtered);
}

async function loadBranchesForSelect() {
    const select = document.getElementById('ddlBranch');
    if (!select) return;

    try {
        const resp = await authFetch('/api/branches');
        if (!resp.ok) {
            console.error('Error cargando sucursales', resp.status);
            showError('No se pudieron cargar las sucursales.');
            return;
        }

        const data = await resp.json();
        branchesCache = data || [];

        select.innerHTML = '<option value="">Seleccione una sucursal</option>';
        branchesCache.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.id;
            opt.textContent = b.name;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error(err);
        showError('Error al cargar sucursales.');
    }
}

async function loadBlocks() {
    showLoading();
    try {
        const resp = await authFetch('/api/blocks');
        if (!resp.ok) {
            console.error('Error bloques', resp.status);
            showError('Error al cargar bloques.');
            return;
        }

        const data = await resp.json();
        blocksCache = data.map(b => ({
            id: b.id,
            name: b.name,
            branchId: b.branchId,
            branchName: b.branchName,
            status: b.status === true || b.status === 1
        }));

        applyFilter();
    } catch (err) {
        console.error(err);
        showError('Error al cargar bloques.');
    } finally {
        hideLoading();
    }
}

function openCreateModal() {
    const form = document.getElementById('blockForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';
    document.getElementById('blockId').value = '';
    document.getElementById('txtBlockName').value = '';
    document.getElementById('ddlBranch').value = '';

    const titleEl = document.getElementById('blockModalTitle');
    if (titleEl) titleEl.textContent = 'Nuevo bloque';

    blockModal.show();
}

function openEditModal(id) {
    const block = blocksCache.find(b => b.id === id);
    if (!block) return;

    const form = document.getElementById('blockForm');
    if (!form) return;

    form.dataset.id = String(block.id);

    document.getElementById('blockId').value = block.id;
    document.getElementById('txtBlockName').value = block.name ?? '';
    const ddl = document.getElementById('ddlBranch');
    if (ddl) ddl.value = block.branchId ?? '';

    const titleEl = document.getElementById('blockModalTitle');
    if (titleEl) titleEl.textContent = 'Editar bloque';

    blockModal.show();
}

async function saveBlock(e) {
    e.preventDefault();

    const form = document.getElementById('blockForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtBlockName').value.trim();
    const ddl = document.getElementById('ddlBranch');

    if (!name) {
        showError('El nombre del bloque es obligatorio.');
        return;
    }
    const branchIdValue = ddl?.value || '';
    if (!branchIdValue) {
        showError('Debes seleccionar una sucursal.');
        return;
    }
    const branchId = parseInt(branchIdValue, 10);

    const payload = {
        name,
        address: null, // Campo opcional del modelo
        branchId,
        status: true
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/blocks' : `/api/blocks/${id}`;

    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando bloque...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        const data = await resp.json();

        showToast(data.message || (isNew ? 'Bloque creado correctamente.' : 'Bloque actualizado correctamente.'), 'success');
        blockModal.hide();
        await loadBlocks();
    } catch (err) {
        console.error(err);
        showError(err.message || 'No se pudo guardar el bloque.');
    } finally {
        hideLoading();
    }
}

async function deleteBlock(id) {
    const ok = await showConfirm('¿Seguro que deseas eliminar este bloque?');
    if (!ok) return;

    showLoading('Eliminando bloque...');
    try {
        const resp = await authFetch(`/api/blocks/${id}`, { method: 'DELETE' });
        const data = await resp.json();

        showToast(data.message || 'Bloque eliminado correctamente.', 'success');
        await loadBlocks();
    } catch (err) {
        console.error(err);
        showError(err.message || 'No se pudo eliminar el bloque.');
    } finally {
        hideLoading();
    }
}

function attachEvents() {
    const btnNew = document.getElementById('btnNewBlock');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('blockForm');
    if (form) form.addEventListener('submit', saveBlock);

    const searchInput = document.getElementById('txtSearch');
    if (searchInput) searchInput.addEventListener('input', applyFilter);

    const tbody = document.getElementById('blocksTableBody');
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const target = e.target;
            if (!(target instanceof HTMLElement)) return;

            const button = target.closest('button');
            if (!button) return;

            const idAttr = button.dataset.id;
            if (!idAttr) return;
            const id = parseInt(idAttr, 10);

            if (button.classList.contains('btn-edit')) {
                openEditModal(id);
            } else if (button.classList.contains('btn-delete')) {
                deleteBlock(id);
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const blockModalEl = document.getElementById('blockModal');
    if (blockModalEl && window.bootstrap) {
        blockModal = new bootstrap.Modal(blockModalEl);
    }

    attachEvents();
    await loadBranchesForSelect();
    await loadBlocks();
});
