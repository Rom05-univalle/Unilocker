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
            <td>
                <button class="btn btn-sm btn-danger" data-id="${pc.id}" data-name="${pc.name}" title="Desregistrar computadora">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </td>
        `;
        
        const btnDelete = tr.querySelector('button[data-id]');
        if (btnDelete) {
            btnDelete.addEventListener('click', () => {
                const id = parseInt(btnDelete.getAttribute('data-id'), 10);
                const name = btnDelete.getAttribute('data-name');
                confirmUnregister(id, name);
            });
        }
        
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
        // Cargar computadoras
        const resp = await authFetch('/api/computers');
        const data = await resp.json();

        // Cargar sesiones activas
        const respSessions = await authFetch('/api/sessions');
        const dataSessions = await respSessions.json();

        // Crear un Set con los IDs de computadoras que tienen sesiones activas
        const activeComputerIds = new Set(
            dataSessions
                .filter(s => s.isActive === true)
                .map(s => s.computerId)
        );

        computersCache = data.map(c => ({
            id: c.id,
            name: c.name,
            uuid: c.uuid,
            status: activeComputerIds.has(c.id), // Estado basado en si tiene sesión activa
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

async function confirmUnregister(id, name) {
    const confirmed = await showConfirm(
        'ADVERTENCIA: Desregistrar Computadora',
        `<div class="alert alert-warning mb-3">
            <strong>Esta acción solo debe usarse en caso de daños o pérdida del equipo.</strong><br>
            Si la computadora está funcionando, desregistre desde el cliente.
        </div>
        <p>¿Está seguro de desregistrar la computadora <strong>${name}</strong>?</p>
        <p class="text-danger small mb-0">Esta acción establecerá el estado de la computadora como inactiva.</p>`
    );

    if (!confirmed) return;

    showLoading('Desregistrando computadora...');
    try {
        const resp = await authFetch(`/api/computers/${id}`, { method: 'DELETE' });
        const data = await resp.json();

        showToast(data.message || 'Computadora desregistrada correctamente.');
        await loadComputers();
    } catch (err) {
        console.error(err);
        const errorMsg = err.message || 'No se pudo desregistrar la computadora.';
        showError(errorMsg);
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
