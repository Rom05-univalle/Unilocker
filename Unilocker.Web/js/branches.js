import { authFetch } from './api.js';
import { initUI, showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let branchModal;
let branchesCache = [];

function renderBranches(rows) {
    const tbody = document.getElementById('branchesTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    rows.forEach(branch => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${branch.name}</td>
            <td>${branch.code ?? ''}</td>
            <td>${branch.address ?? ''}</td>
            <td>
                <span class="badge ${branch.status ? 'bg-success' : 'bg-secondary'}">
                    ${branch.status ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${branch.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${branch.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function applyFilter() {
    const search = document.getElementById('txtSearch')?.value?.toLowerCase() || '';
    const filtered = branchesCache.filter(b =>
        b.name.toLowerCase().includes(search) ||
        (b.code ?? '').toLowerCase().includes(search) ||
        (b.address ?? '').toLowerCase().includes(search)
    );
    renderBranches(filtered);
}

async function loadBranches() {
    showLoading();
    try {
        const resp = await authFetch('/api/branches');
        const data = await resp.json();

        branchesCache = data.map(b => ({
            id: b.id,
            name: b.name,
            code: b.code,
            address: b.address,
            status: b.status === true || b.status === 1
        }));

        applyFilter();  // esto llama a renderBranches(...)
    } catch (err) {
        console.error(err);
        showError('Error al cargar sucursales.');
    } finally {
        hideLoading();
    }
}


function openCreateModal() {
    const form = document.getElementById('branchForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('branchId').value = '';
    document.getElementById('txtBranchName').value = '';
    document.getElementById('txtBranchCode').value = '';
    document.getElementById('txtBranchAddress').value = '';
    const chk = document.getElementById('chkStatus');
    if (chk) chk.checked = true;

    const titleEl = document.getElementById('branchModalTitle');
    if (titleEl) titleEl.textContent = 'Nueva sucursal';

    branchModal.show();
}

function openEditModal(id) {
    const branch = branchesCache.find(b => b.id === id);
    if (!branch) return;

    const form = document.getElementById('branchForm');
    if (!form) return;

    form.dataset.id = String(branch.id);

    document.getElementById('branchId').value = branch.id;
    document.getElementById('txtBranchName').value = branch.name ?? '';
    document.getElementById('txtBranchCode').value = branch.code ?? '';
    document.getElementById('txtBranchAddress').value = branch.address ?? '';
    const chk = document.getElementById('chkStatus');
    if (chk) chk.checked = !!branch.status;

    const titleEl = document.getElementById('branchModalTitle');
    if (titleEl) titleEl.textContent = 'Editar sucursal';

    branchModal.show();
}

async function saveBranch(e) {
    e.preventDefault();

    const form = document.getElementById('branchForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtBranchName').value.trim();
    const code = document.getElementById('txtBranchCode').value.trim();
    const address = document.getElementById('txtBranchAddress').value.trim();
    const chk = document.getElementById('chkStatus');

    if (!name) {
        showError('El nombre es obligatorio.');
        return;
    }
    if (!code) {
        showError('El código es obligatorio.');
        return;
    }
    if (!address) {
        showError('La dirección es obligatoria.');
        return;
    }

    const payload = {
        name,
        code,
        address,
        status: chk ? chk.checked : true
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/branches' : `/api/branches/${id}`;

    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando sucursal...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        await resp.text(); // no usamos el cuerpo, solo aseguramos que se lea

        showToast(isNew ? 'Sucursal creada correctamente.' : 'Sucursal actualizada correctamente.');
        branchModal.hide();
        await loadBranches();
    } catch (err) {
        console.error(err);
        showError('No se pudo guardar la sucursal.');
    } finally {
        hideLoading();
    }
}

async function deleteBranch(id) {
    const ok = await showConfirm('¿Seguro que deseas eliminar esta sucursal? (Se eliminaran todos los registros relacionados)');
    if (!ok) return;

    showLoading('Eliminando sucursal...');
    try {
        const resp = await authFetch(`/api/branches/${id}`, { method: 'DELETE' });

        // Tu API devuelve 204, así que con llegar aquí ya está OK
        showToast('Sucursal eliminada correctamente.');
        await loadBranches();   // ← esta llamada es la que refresca la tabla
    } catch (err) {
        console.error(err);
        showError('No se pudo eliminar la sucursal.');
    } finally {
        hideLoading();
    }
}


function attachEvents() {
    const btnNew = document.getElementById('btnNewBranch');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('branchForm');
    if (form) form.addEventListener('submit', saveBranch);

    const searchInput = document.getElementById('txtSearch');
    if (searchInput) searchInput.addEventListener('input', applyFilter);

    const tbody = document.getElementById('branchesTableBody');
    if (tbody) {
        tbody.addEventListener('click', (e) => {
            const target = e.target;
            if (!(target instanceof HTMLElement)) return;

            const button = target.closest('button');
            if (!button) return;

            const idAttr = button.dataset.id;
            if (!idAttr) return;
            const id = parseInt(idAttr, 10);
            if (Number.isNaN(id)) return;

            if (button.classList.contains('btn-edit')) {
                openEditModal(id);
            } else if (button.classList.contains('btn-delete')) {
                deleteBranch(id);
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    initUI('Gestión', 'Sucursales');

    const branchModalEl = document.getElementById('branchModal');
    if (branchModalEl && window.bootstrap) {
        branchModal = new bootstrap.Modal(branchModalEl);
    }

    attachEvents();
    await loadBranches();
});
