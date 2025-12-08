import { API_BASE_URL, authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let computerModal;
let computersCache = [];
let branchesCache = [];
let blocksCache = [];
let classroomsCache = [];

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
    const filterBranch = document.getElementById('filterBranch');
    const filterBlock = document.getElementById('filterBlock');
    const filterClassroom = document.getElementById('filterClassroom');
    const filterStatus = document.getElementById('filterStatus');

    const branchId = filterBranch ? parseInt(filterBranch.value || '0', 10) : 0;
    const blockId = filterBlock ? parseInt(filterBlock.value || '0', 10) : 0;
    const classroomId = filterClassroom ? parseInt(filterClassroom.value || '0', 10) : 0;
    const statusFilter = filterStatus ? filterStatus.value : '';

    let filtered = [...computersCache];
    
    if (branchId > 0) {
        filtered = filtered.filter(c => c.branchId === branchId);
    }
    if (blockId > 0) {
        filtered = filtered.filter(c => c.blockId === blockId);
    }
    if (classroomId > 0) {
        filtered = filtered.filter(c => c.classroomId === classroomId);
    }
    if (statusFilter === 'active') {
        filtered = filtered.filter(c => c.status === true);
    } else if (statusFilter === 'inactive') {
        filtered = filtered.filter(c => c.status === false);
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
        console.error(err);
        showError(err.message || 'No se pudo desregistrar la computadora.');
    } finally {
        hideLoading();
    }
}

// EVENTOS
function attachEvents() {
    const filterBranch = document.getElementById('filterBranch');
    const filterBlock = document.getElementById('filterBlock');
    const filterClassroom = document.getElementById('filterClassroom');
    const filterStatus = document.getElementById('filterStatus');

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

    if (filterStatus) {
        filterStatus.addEventListener('change', applyFilter);
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
