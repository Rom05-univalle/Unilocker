import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let roleModal;
let rolesCache = [];

// RENDER TABLA

function renderRoles(items) {
    const tbody = document.getElementById('rolesTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(r => {
        const statusBadge = r.status ? 'Activo' : 'Inactivo';
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${r.name}</td>
            <td>${r.description ?? ''}</td>
            <td>
                <span class="badge ${r.status ? 'bg-success' : 'bg-secondary'}">
                    ${statusBadge}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${r.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${r.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// CARGA ROLES

async function loadRoles() {
    showLoading('Cargando roles...');
    try {
        const resp = await authFetch('/api/roles');
        const data = await resp.json();

        rolesCache = data.map(r => ({
            id: r.id,
            name: r.name,
            description: r.description,
            status: r.status === true || r.status === 1
        }));

        renderRoles(rolesCache);
    } catch (err) {
        console.error(err);
        showError('Error al cargar roles.');
    } finally {
        hideLoading();
    }
}

// MODAL CREAR / EDITAR

function openCreateModal() {
    const form = document.getElementById('roleForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('roleId').value = '';
    document.getElementById('txtRoleName').value = '';
    document.getElementById('txtRoleDescription').value = '';

    const chk = document.getElementById('chkRoleStatus');
    if (chk) chk.checked = true;

    const titleEl = document.getElementById('roleModalTitle');
    if (titleEl) titleEl.textContent = 'Nuevo rol';

    roleModal.show();
}

function openEditModal(id) {
    const r = rolesCache.find(x => x.id === id);
    if (!r) return;

    const form = document.getElementById('roleForm');
    if (!form) return;

    form.dataset.id = String(r.id);

    document.getElementById('roleId').value = r.id;
    document.getElementById('txtRoleName').value = r.name ?? '';
    document.getElementById('txtRoleDescription').value = r.description ?? '';

    const chk = document.getElementById('chkRoleStatus');
    if (chk) chk.checked = !!r.status;

    const titleEl = document.getElementById('roleModalTitle');
    if (titleEl) titleEl.textContent = 'Editar rol';

    roleModal.show();
}

// GUARDAR (CREATE / UPDATE)

async function saveRole(e) {
    e.preventDefault();

    const form = document.getElementById('roleForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtRoleName').value.trim();
    const description = document.getElementById('txtRoleDescription').value.trim();
    const chk = document.getElementById('chkRoleStatus');

    if (!name) {
        showError('El nombre es obligatorio.');
        return;
    }

    const payload = {
        name,
        description: description || null,
        status: chk ? chk.checked : true
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/roles' : `/api/roles/${id}`;
    
    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando rol...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        const text = await resp.text();
        if (!resp.ok) {
            console.error('Error guardando rol', resp.status, text);
            showError(text || 'No se pudo guardar el rol.');
            return;
        }

        showToast(isNew ? 'Rol creado correctamente.' : 'Rol actualizado correctamente.');
        roleModal.hide();
        await loadRoles();
    } catch (err) {
        console.error(err);
        showError('No se pudo guardar el rol.');
    } finally {
        hideLoading();
    }
}

// ELIMINAR (borrado lógico según backend)

async function deleteRole(id) {
    // Buscar el rol a eliminar en la caché
    const roleToDelete = rolesCache.find(r => r.id === id);
    if (!roleToDelete) {
        showError('Rol no encontrado.');
        return;
    }

    // Validación: No permitir eliminar el rol Admin
    if (roleToDelete.name && roleToDelete.name.toLowerCase() === 'admin') {
        showError('No puedes eliminar el rol de Administrador.');
        return;
    }

    const ok = await showConfirm('¿Seguro que quieres eliminar este rol? (También se eliminarán todos los usuarios con este rol)');
    if (!ok) return;

    showLoading('Eliminando rol...');
    try {
        const resp = await authFetch(`/api/roles/${id}`, { method: 'DELETE' });
        const text = await resp.text();

        if (!resp.ok && resp.status !== 204) {
            console.error('Error eliminando rol', resp.status, text);
            showError(text || 'No se pudo eliminar el rol.');
            return;
        }

        showToast('Rol eliminado correctamente.');
        await loadRoles();
    } catch (err) {
        console.error(err);
        showError('No se pudo eliminar el rol.');
    } finally {
        hideLoading();
    }
}

// EVENTOS

function attachEvents() {
    const btnNew = document.getElementById('btnNewRole');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('roleForm');
    if (form) form.addEventListener('submit', saveRole);

    const tbody = document.getElementById('rolesTableBody');
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
                deleteRole(id);
            }
        });
    }
}

// INICIALIZACIÓN

document.addEventListener('DOMContentLoaded', async () => {
    const modalEl = document.getElementById('roleModal');
    if (modalEl && window.bootstrap) {
        roleModal = new bootstrap.Modal(modalEl);
    }

    attachEvents();
    await loadRoles();
});
