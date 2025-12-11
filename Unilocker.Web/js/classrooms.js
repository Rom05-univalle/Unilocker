import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let classroomModal;
let classroomsCache = [];
let branchesCache = [];
let blocksCache = [];

function renderClassrooms(items) {
    const tbody = document.getElementById('classroomsTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(c => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${c.name}</td>
            <td>${c.blockName ?? ''}</td>
            <td>${c.branchName ?? ''}</td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${c.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${c.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function applyFilter() {
    const search = document.getElementById('txtSearch')?.value?.toLowerCase() || '';
    const filtered = classroomsCache.filter(c =>
        c.name.toLowerCase().includes(search) ||
        (c.blockName ?? '').toLowerCase().includes(search) ||
        (c.branchName ?? '').toLowerCase().includes(search)
    );
    renderClassrooms(filtered);
}

async function loadBranchesForSelect() {
    const select = document.getElementById('ddlBranch');
    if (!select) return;

    try {
        const resp = await authFetch('/api/branches');
        if (!resp.ok) {
            console.error('Error sucursales', resp.status);
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
        window.handleApiError(err, 'Error al cargar sucursales.');
    }
}

async function loadBlocksForSelect(branchId) {
    const select = document.getElementById('ddlBlock');
    if (!select) return;

    if (!branchId) {
        select.innerHTML = '<option value="">Seleccione un bloque</option>';
        blocksCache = [];
        return;
    }

    try {
        const resp = await authFetch(`/api/blocks?branchId=${branchId}`);
        if (!resp.ok) {
            console.error('Error bloques por sucursal', resp.status);
            showError('No se pudieron cargar los bloques.');
            return;
        }

        const data = await resp.json();
        blocksCache = data || [];

        select.innerHTML = '<option value="">Seleccione un bloque</option>';
        blocksCache.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.id;
            opt.textContent = b.name;
            select.appendChild(opt);
        });
    } catch (err) {
        window.handleApiError(err, 'Error al cargar bloques.');
    }
}

async function loadClassrooms() {
    showLoading();
    try {
        const resp = await authFetch('/api/classrooms');
        if (!resp.ok) {
            console.error('Error aulas', resp.status);
            showError('Error al cargar aulas.');
            return;
        }

        const data = await resp.json();
        classroomsCache = data.map(c => ({
            id: c.id,
            name: c.name,
            capacity: c.capacity,
            blockId: c.blockId,
            blockName: c.blockName,
            branchId: c.branchId,
            branchName: c.branchName,
            status: c.status === true || c.status === 1
        }));

        applyFilter();
    } catch (err) {
        window.handleApiError(err, 'Error al cargar aulas.');
    } finally {
        hideLoading();
    }
}

function openCreateModal() {
    const form = document.getElementById('classroomForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('classroomId').value = '';
    document.getElementById('txtClassroomName').value = '';
    document.getElementById('txtCapacity').value = '';
    const ddlBranch = document.getElementById('ddlBranch');
    const ddlBlock = document.getElementById('ddlBlock');
    if (ddlBranch) ddlBranch.value = '';
    if (ddlBlock) ddlBlock.innerHTML = '<option value="">Seleccione un bloque</option>';

    const titleEl = document.getElementById('classroomModalTitle');
    if (titleEl) titleEl.textContent = 'Nueva aula';

    classroomModal.show();
}

async function openEditModal(id) {
    const classroom = classroomsCache.find(c => c.id === id);
    if (!classroom) return;

    const form = document.getElementById('classroomForm');
    if (!form) return;

    form.dataset.id = String(classroom.id);

    document.getElementById('classroomId').value = classroom.id;
    document.getElementById('txtClassroomName').value = classroom.name ?? '';
    document.getElementById('txtCapacity').value = classroom.capacity ?? '';
    const ddlBranch = document.getElementById('ddlBranch');
    const ddlBlock = document.getElementById('ddlBlock');

    if (ddlBranch) ddlBranch.value = classroom.branchId ?? '';
    await loadBlocksForSelect(classroom.branchId);

    if (ddlBlock) {
        const existsInBranch = blocksCache.some(b => b.id === classroom.blockId);
        ddlBlock.value = existsInBranch ? classroom.blockId : '';
    }

    const titleEl = document.getElementById('classroomModalTitle');
    if (titleEl) titleEl.textContent = 'Editar aula';

    classroomModal.show();
}

async function saveClassroom(e) {
    e.preventDefault();

    const form = document.getElementById('classroomForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtClassroomName').value.trim();
    const capacityInput = document.getElementById('txtCapacity');
    const capacity = capacityInput?.value ? parseInt(capacityInput.value, 10) : null;
    const ddlBranch = document.getElementById('ddlBranch');
    const ddlBlock = document.getElementById('ddlBlock');

    if (!name) {
        showError('El nombre del aula es obligatorio.');
        return;
    }

    // Validar capacidad máxima
    if (capacity !== null && capacity > 100) {
        showError('La capacidad no puede ser mayor a 100.');
        return;
    }

    const branchIdValue = ddlBranch?.value || '';
    if (!branchIdValue) {
        showError('Debes seleccionar una sucursal.');
        return;
    }
    const blockIdValue = ddlBlock?.value || '';
    if (!blockIdValue) {
        showError('Debes seleccionar un bloque.');
        return;
    }

    const payload = {
        name,
        capacity,
        blockId: parseInt(blockIdValue, 10),
        status: true
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/classrooms' : `/api/classrooms/${id}`;

    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando aula...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        const data = await resp.json();

        showToast(data.message || (isNew ? 'Aula creada correctamente.' : 'Aula actualizada correctamente.'), 'success');
        classroomModal.hide();
        await loadClassrooms();
    } catch (err) {
        window.handleApiError(err, err.message || 'No se pudo guardar el aula.');
    } finally {
        hideLoading();
    }
}

async function deleteClassroom(id) {
    const ok = await showConfirm('¿Seguro que deseas eliminar esta aula?\n\n(Se eliminarán todos los registros relacionados)');
    if (!ok) return;

    showLoading('Eliminando aula...');
    try {
        const resp = await authFetch(`/api/classrooms/${id}`, { method: 'DELETE' });
        const data = await resp.json();

        showToast(data.message || 'Aula eliminada correctamente.', 'success');
        await loadClassrooms();
    } catch (err) {
        window.handleApiError(err, err.message || 'No se pudo eliminar el aula.');
    } finally {
        hideLoading();
    }
}

function attachEvents() {
    const btnNew = document.getElementById('btnNewClassroom');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('classroomForm');
    if (form) form.addEventListener('submit', saveClassroom);

    const searchInput = document.getElementById('txtSearch');
    if (searchInput) searchInput.addEventListener('input', applyFilter);

    const ddlBranch = document.getElementById('ddlBranch');
    if (ddlBranch) {
        ddlBranch.addEventListener('change', async (e) => {
            const value = e.target.value;
            const branchId = value ? parseInt(value, 10) : null;
            await loadBlocksForSelect(branchId);
        });
    }

    const tbody = document.getElementById('classroomsTableBody');
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
                deleteClassroom(id);
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const classroomModalEl = document.getElementById('classroomModal');
    if (classroomModalEl && window.bootstrap) {
        classroomModal = new bootstrap.Modal(classroomModalEl);
    }

    attachEvents();
    await loadBranchesForSelect();
    await loadClassrooms();
});
