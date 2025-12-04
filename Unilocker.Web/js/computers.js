import { API_BASE_URL, authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let computerModal;
let computersCache = [];
let classroomsOptions = [];

function renderComputers(items) {
    const tbody = document.getElementById('computersTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(pc => {
        const statusBadge = pc.status ? 'Activa' : 'Inactiva';
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${pc.id}</td>
            <td>${pc.branchName ?? ''}</td>
            <td>${pc.blockName ?? ''}</td>
            <td>${pc.classroomName ?? ''}</td>
            <td>${pc.name}</td>
            <td>${pc.uuid ?? ''}</td>
            <td>
                <span class="badge ${pc.status ? 'bg-success' : 'bg-secondary'}">
                    ${statusBadge}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${pc.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${pc.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function applyFilter() {
    const sel = document.getElementById('filterClassroom');
    const classroomId = sel ? parseInt(sel.value || '0', 10) : 0;

    let filtered = [...computersCache];
    if (classroomId > 0) {
        filtered = filtered.filter(c => c.classroomId === classroomId);
    }
    renderComputers(filtered);
}

async function loadComputers() {
    showLoading('Cargando computadoras...');
    try {
        const resp = await authFetch('/api/computers');
        const data = await resp.json();

        computersCache = data.map(c => ({
            id: c.id,
            name: c.name,
            uuid: c.uuid,
            status: c.status === true || c.status === 1,
            classroomId: c.classroomId,
            classroomName: c.classroomName,
            blockId: c.blockId,
            blockName: c.blockName,
            branchId: c.branchId,
            branchName: c.branchName
        }));

        applyFilter();
    } catch (err) {
        console.error(err);
        showError('Error al cargar computadoras.');
    } finally {
        hideLoading();
    }
}

// CARGA DE AULAS PARA SELECT
async function loadClassrooms() {
    try {
        const resp = await authFetch('/api/classrooms');
        const data = await resp.json();

        classroomsOptions = data.map(c => ({
            id: c.id,
            name: c.name,
            blockName: c.blockName,
            branchName: c.branchName
        }));

        populateClassroomSelect();
        populateFilterClassroomSelect();
    } catch (err) {
        console.error(err);
        showError('Error al cargar aulas.');
    }
}

function populateClassroomSelect(selectedId = null) {
    const sel = document.getElementById('selClassroom');
    if (!sel) return;

    sel.innerHTML = '<option value="">Seleccione...</option>';

    classroomsOptions.forEach(c => {
        const opt = document.createElement('option');
        opt.value = c.id;
        opt.textContent = `${c.branchName} - ${c.blockName} - ${c.name}`;
        if (selectedId && selectedId === c.id) {
            opt.selected = true;
        }
        sel.appendChild(opt);
    });
}

function populateFilterClassroomSelect() {
    const sel = document.getElementById('filterClassroom');
    if (!sel) return;

    sel.innerHTML = '<option value="0">Todas</option>';
    classroomsOptions.forEach(c => {
        const opt = document.createElement('option');
        opt.value = c.id;
        opt.textContent = `${c.branchName} - ${c.blockName} - ${c.name}`;
        sel.appendChild(opt);
    });
}

// MODAL

function openCreateModal() {
    const form = document.getElementById('computerForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('computerId').value = '';
    document.getElementById('txtComputerName').value = '';
    document.getElementById('txtComputerUuid').value = '';

    const chk = document.getElementById('chkComputerStatus');
    if (chk) chk.checked = true;

    populateClassroomSelect();

    const titleEl = document.getElementById('computerModalTitle');
    if (titleEl) titleEl.textContent = 'Nueva computadora';

    computerModal.show();
}

function openEditModal(id) {
    const pc = computersCache.find(c => c.id === id);
    if (!pc) return;

    const form = document.getElementById('computerForm');
    if (!form) return;
    form.dataset.id = String(pc.id);

    document.getElementById('computerId').value = pc.id;
    document.getElementById('txtComputerName').value = pc.name ?? '';
    document.getElementById('txtComputerUuid').value = pc.uuid ?? '';

    const chk = document.getElementById('chkComputerStatus');
    if (chk) chk.checked = !!pc.status;

    populateClassroomSelect(pc.classroomId);

    const titleEl = document.getElementById('computerModalTitle');
    if (titleEl) titleEl.textContent = 'Editar computadora';

    computerModal.show();
}

async function saveComputer(e) {
    e.preventDefault();

    const form = document.getElementById('computerForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtComputerName').value.trim();
    let uuid = document.getElementById('txtComputerUuid').value.trim();
    const selClassroom = document.getElementById('selClassroom');
    const chk = document.getElementById('chkComputerStatus');

    const classroomId = selClassroom ? parseInt(selClassroom.value || '0', 10) : 0;

    if (!name) {
        showError('El nombre es obligatorio.');
        return;
    }
    if (!classroomId) {
        showError('Debes seleccionar un aula.');
        return;
    }

    // Generar UUID válido si está vacío o mal formado
    try {
        if (!uuid || !/^[0-9a-fA-F-]{36}$/.test(uuid)) {
            uuid = crypto.randomUUID();
            document.getElementById('txtComputerUuid').value = uuid;
        }
    } catch {
        showError('Este navegador no soporta crypto.randomUUID().');
        return;
    }

    const payload = {
        name,
        uuid,
        status: chk ? chk.checked : true,
        classroomId
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/computers' : `/api/computers/${id}`;

    showLoading('Guardando computadora...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        await resp.text();

        showToast(isNew ? 'Computadora creada correctamente.' : 'Computadora actualizada correctamente.');
        computerModal.hide();
        await loadComputers();
    } catch (err) {
        console.error(err);
        showError('No se pudo guardar la computadora.');
    } finally {
        hideLoading();
    }
}

async function deleteComputer(id) {
    const ok = await showConfirm('Seguro que quieres eliminar esta computadora? (borrado lógico).');
    if (!ok) return;

    showLoading('Eliminando computadora...');
    try {
        const resp = await authFetch(`/api/computers/${id}`, { method: 'DELETE' });

        if (!resp.ok && resp.status !== 204) {
            const text = await resp.text();
            console.error('Error eliminando computadora', resp.status, text);
            showError(text || 'No se pudo eliminar la computadora.');
            return;
        }

        showToast('Computadora eliminada correctamente.');
        await loadComputers();
    } catch (err) {
        console.error(err);
        showError('No se pudo eliminar la computadora.');
    } finally {
        hideLoading();
    }
}

function attachEvents() {
    const btnNew = document.getElementById('btnNewComputer');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('computerForm');
    if (form) form.addEventListener('submit', saveComputer);

    const selFilter = document.getElementById('filterClassroom');
    if (selFilter) selFilter.addEventListener('change', applyFilter);

    const tbody = document.getElementById('computersTableBody');
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
                deleteComputer(id);
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const modalEl = document.getElementById('computerModal');
    if (modalEl && window.bootstrap) {
        computerModal = new bootstrap.Modal(modalEl);
    }

    attachEvents();
    await loadClassrooms();
    await loadComputers();
});
