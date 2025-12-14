import { API_BASE_URL, authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let statusModal;
let computersCache = [];
let branchesCache = [];
let blocksCache = [];
let classroomsCache = [];

function getStatusBadge(computerStatus) {
    const badges = {
        'Active': '<span class="badge bg-success">Activa</span>',
        'Maintenance': '<span class="badge bg-warning text-dark">Mantenimiento</span>',
        'Decommissioned': '<span class="badge bg-danger">Dada de Baja</span>'
    };
    return badges[computerStatus] || '<span class="badge bg-secondary">Desconocido</span>';
}

function getInUseBadge(inUse) {
    return inUse 
        ? '<span class="badge bg-primary"><i class="fa-solid fa-circle-check me-1"></i>En Uso</span>'
        : '<span class="badge bg-secondary"><i class="fa-solid fa-circle me-1"></i>Disponible</span>';
}

function renderComputers(items) {
    const tbody = document.getElementById('computersTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(pc => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${pc.classroomInfo?.branchName ?? ''}</td>
            <td>${pc.classroomInfo?.blockName ?? ''}</td>
            <td>${pc.classroomInfo?.name ?? ''}</td>
            <td>${pc.name}</td>
            <td><small class="text-muted">${pc.operatingSystem ?? 'N/A'}</small></td>
            <td>${getInUseBadge(pc.inUse)}</td>
            <td>${getStatusBadge(pc.computerStatus)}</td>
            <td>
                <button class="btn btn-sm btn-primary me-1" data-action="status" data-id="${pc.id}" data-name="${pc.name}" data-status="${pc.computerStatus}" title="Cambiar estado">
                    <i class="fa-solid fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger" data-action="delete" data-id="${pc.id}" data-name="${pc.name}" title="Desregistrar">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </td>
        `;
        
        // Botón cambiar estado
        const btnStatus = tr.querySelector('button[data-action="status"]');
        if (btnStatus) {
            btnStatus.addEventListener('click', () => {
                const id = parseInt(btnStatus.getAttribute('data-id'), 10);
                const name = btnStatus.getAttribute('data-name');
                const status = btnStatus.getAttribute('data-status');
                openStatusModal(id, name, status);
            });
        }
        
        // Botón desregistrar
        const btnDelete = tr.querySelector('button[data-action="delete"]');
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
    const filterBranch = document.getElementById('filterBranch');
    const filterBlock = document.getElementById('filterBlock');
    const filterClassroom = document.getElementById('filterClassroom');
    const filterInUse = document.getElementById('filterInUse');
    const filterComputerStatus = document.getElementById('filterComputerStatus');

    const branchId = filterBranch ? parseInt(filterBranch.value || '0', 10) : 0;
    const blockId = filterBlock ? parseInt(filterBlock.value || '0', 10) : 0;
    const classroomId = filterClassroom ? parseInt(filterClassroom.value || '0', 10) : 0;
    const inUseFilter = filterInUse ? filterInUse.value : '';
    const statusFilter = filterComputerStatus ? filterComputerStatus.value : '';

    let filtered = [...computersCache];
    
    if (branchId > 0) {
        filtered = filtered.filter(c => c.classroomInfo?.branchId === branchId);
    }
    if (blockId > 0) {
        filtered = filtered.filter(c => c.classroomInfo?.blockId === blockId);
    }
    if (classroomId > 0) {
        filtered = filtered.filter(c => c.classroomInfo?.id === classroomId);
    }
    if (inUseFilter === 'true') {
        filtered = filtered.filter(c => c.inUse === true);
    } else if (inUseFilter === 'false') {
        filtered = filtered.filter(c => c.inUse === false);
    }
    if (statusFilter) {
        filtered = filtered.filter(c => c.computerStatus === statusFilter);
    }
    
    renderComputers(filtered);
}

async function loadComputers() {
    showLoading('Cargando computadoras...');
    try {
        const resp = await authFetch('/api/computers');
        const data = await resp.json();
        computersCache = data;
        applyFilter();
    } catch (err) {
        window.handleApiError(err, 'Error al cargar computadoras.');
    } finally {
        hideLoading();
    }
}

// CARGA DE DATOS
async function loadBranches() {
    try {
        const resp = await authFetch('/api/branches');
        const data = await resp.json();
        branchesCache = data.filter(b => b.status === true || b.status === 1);
        populateBranchSelect();
    } catch (err) {
        console.error(err);
    }
}

async function loadBlocks() {
    try {
        const resp = await authFetch('/api/blocks');
        const data = await resp.json();
        blocksCache = data.filter(b => b.status === true || b.status === 1);
    } catch (err) {
        console.error(err);
    }
}

async function loadClassrooms() {
    try {
        const resp = await authFetch('/api/classrooms');
        const data = await resp.json();
        classroomsCache = data.filter(c => c.status === true || c.status === 1);
    } catch (err) {
        console.error(err);
    }
}

function populateBranchSelect() {
    const sel = document.getElementById('filterBranch');
    if (!sel) return;
    
    sel.innerHTML = '<option value="0">Todas</option>';
    branchesCache.forEach(b => {
        const opt = document.createElement('option');
        opt.value = b.id;
        opt.textContent = b.name;
        sel.appendChild(opt);
    });
}

function populateBlockSelect(branchId) {
    const sel = document.getElementById('filterBlock');
    if (!sel) return;
    
    sel.innerHTML = '<option value="0">Todos</option>';
    sel.disabled = branchId === 0;
    
    if (branchId > 0) {
        const filtered = blocksCache.filter(b => b.branchId === branchId);
        filtered.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.id;
            opt.textContent = b.name;
            sel.appendChild(opt);
        });
    }
}

function populateClassroomSelect(blockId) {
    const sel = document.getElementById('filterClassroom');
    if (!sel) return;
    
    sel.innerHTML = '<option value="0">Todas</option>';
    sel.disabled = blockId === 0;
    
    if (blockId > 0) {
        const filtered = classroomsCache.filter(c => c.blockId === blockId);
        filtered.forEach(c => {
            const opt = document.createElement('option');
            opt.value = c.id;
            opt.textContent = c.name;
            sel.appendChild(opt);
        });
    }
}

// CAMBIAR ESTADO DE COMPUTADORA
function openStatusModal(id, name, currentStatus) {
    const modal = document.getElementById('statusModal');
    if (!modal) return;

    document.getElementById('statusComputerId').value = id;
    document.getElementById('statusComputerName').textContent = name;
    document.getElementById('selectComputerStatus').value = currentStatus;

    if (!statusModal) {
        statusModal = new bootstrap.Modal(modal);
    }
    statusModal.show();
}

async function updateComputerStatus() {
    const id = document.getElementById('statusComputerId').value;
    const newStatus = document.getElementById('selectComputerStatus').value;

    showLoading('Actualizando estado...');
    try {
        const resp = await authFetch(`/api/computers/${id}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ computerStatus: newStatus })
        });
        
        const data = await resp.json();
        showToast(data.message || 'Estado actualizado correctamente.');
        
        if (statusModal) statusModal.hide();
        await loadComputers();
    } catch (err) {
        window.handleApiError(err, 'Error al actualizar el estado de la computadora.');
    } finally {
        hideLoading();
    }
}

// ELIMINAR/DESREGISTRAR COMPUTADORA
async function confirmUnregister(id, name) {
    const ok = await showConfirm(`¿Seguro que quieres desregistrar la computadora "${name}"? Se cerrarán las sesiones activas si las hay.`);
    if (!ok) return;

    showLoading('Desregistrando computadora...');
    try {
        const resp = await authFetch(`/api/computers/${id}`, { method: 'DELETE' });
        const data = await resp.json();

        showToast(data.message || 'Computadora desregistrada correctamente.');
        await loadComputers();
    } catch (err) {
        window.handleApiError(err, err.message || 'No se pudo desregistrar la computadora.');
    } finally {
        hideLoading();
    }
}

// EVENTOS
function attachEvents() {
    const filterBranch = document.getElementById('filterBranch');
    const filterBlock = document.getElementById('filterBlock');
    const filterClassroom = document.getElementById('filterClassroom');
    const filterInUse = document.getElementById('filterInUse');
    const filterComputerStatus = document.getElementById('filterComputerStatus');
    const statusForm = document.getElementById('statusForm');

    if (filterBranch) {
        filterBranch.addEventListener('change', () => {
            const branchId = parseInt(filterBranch.value || '0', 10);
            populateBlockSelect(branchId);
            populateClassroomSelect(0);
            applyFilter();
        });
    }

    if (filterBlock) {
        filterBlock.addEventListener('change', () => {
            const blockId = parseInt(filterBlock.value || '0', 10);
            populateClassroomSelect(blockId);
            applyFilter();
        });
    }

    if (filterClassroom) {
        filterClassroom.addEventListener('change', applyFilter);
    }

    if (filterInUse) {
        filterInUse.addEventListener('change', applyFilter);
    }

    if (filterComputerStatus) {
        filterComputerStatus.addEventListener('change', applyFilter);
    }

    if (statusForm) {
        statusForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            await updateComputerStatus();
        });
    }
}

// INICIALIZACIÓN
document.addEventListener('DOMContentLoaded', async () => {
    attachEvents();
    await loadBranches();
    await loadBlocks();
    await loadClassrooms();
    await loadComputers();
});
