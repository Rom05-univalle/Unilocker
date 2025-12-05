import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let userModal;
let usersCache = [];
let rolesOptions = [];

// RENDER TABLA

function renderUsers(items) {
    const tbody = document.getElementById('usersTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(u => {
        const statusBadge = u.status ? 'Activo' : 'Inactivo';
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${u.id}</td>
            <td>${u.username}</td>
            <td>${u.email}</td>
            <td>${u.roleName ?? ''}</td>
            <td>
                <span class="badge ${u.status ? 'bg-success' : 'bg-secondary'}">
                    ${statusBadge}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${u.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${u.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// CARGA USERS

async function loadUsers() {
    showLoading('Cargando usuarios...');
    try {
        const resp = await authFetch('/api/users');
        const data = await resp.json();

        usersCache = data.map(u => ({
            id: u.id,
            username: u.username,
            email: u.email,
            status: u.status === true || u.status === 1,
            roleId: u.roleId,
            roleName: u.roleName
        }));

        renderUsers(usersCache);
    } catch (err) {
        console.error(err);
        showError('Error al cargar usuarios.');
    } finally {
        hideLoading();
    }
}

// CARGA ROLES PARA SELECT

async function loadRoles() {
    try {
        const resp = await authFetch('/api/roles');
        const data = await resp.json();

        rolesOptions = data
            .filter(r => r.status === true || r.status === 1)
            .map(r => ({
                id: r.id,
                name: r.name
            }));

        populateRolesSelect();
    } catch (err) {
        console.error(err);
        showError('Error al cargar roles.');
    }
}

function populateRolesSelect(selectedId = null) {
    const sel = document.getElementById('selUserRole');
    if (!sel) return;

    sel.innerHTML = '<option value="">Seleccione rol...</option>';

    rolesOptions.forEach(r => {
        const opt = document.createElement('option');
        opt.value = r.id;
        opt.textContent = r.name;
        if (selectedId && selectedId === r.id) {
            opt.selected = true;
        }
        sel.appendChild(opt);
    });
}

// MODAL CREAR / EDITAR

function openCreateModal() {
    const form = document.getElementById('userForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('userId').value = '';
    document.getElementById('txtUsername').value = '';
    document.getElementById('txtEmail').value = '';
    document.getElementById('txtPassword').value = '';

    const chk = document.getElementById('chkUserStatus');
    if (chk) chk.checked = true;

    populateRolesSelect();

    const titleEl = document.getElementById('userModalTitle');
    if (titleEl) titleEl.textContent = 'Nuevo usuario';

    userModal.show();
}

function openEditModal(id) {
    const u = usersCache.find(x => x.id === id);
    if (!u) return;

    const form = document.getElementById('userForm');
    if (!form) return;

    form.dataset.id = String(u.id);

    document.getElementById('userId').value = u.id;
    document.getElementById('txtUsername').value = u.username ?? '';
    document.getElementById('txtEmail').value = u.email ?? '';
    document.getElementById('txtPassword').value = ''; // vacío: solo cambia si escribe algo

    const chk = document.getElementById('chkUserStatus');
    if (chk) chk.checked = !!u.status;

    populateRolesSelect(u.roleId);

    const titleEl = document.getElementById('userModalTitle');
    if (titleEl) titleEl.textContent = 'Editar usuario';

    userModal.show();
}

// GUARDAR (CREATE / UPDATE)

async function saveUser(e) {
    e.preventDefault();

    const form = document.getElementById('userForm');
    const id = form.dataset.id;

    const username = document.getElementById('txtUsername').value.trim();
    const email = document.getElementById('txtEmail').value.trim();
    const password = document.getElementById('txtPassword').value;
    const selRole = document.getElementById('selUserRole');
    const chk = document.getElementById('chkUserStatus');

    const roleId = selRole ? parseInt(selRole.value || '0', 10) : 0;

    if (!username) {
        showError('El username es obligatorio.');
        return;
    }
    if (!email) {
        showError('El email es obligatorio.');
        return;
    }
    if (!id && !password) {
        showError('La contraseña es obligatoria para un usuario nuevo.');
        return;
    }
    if (!roleId) {
        showError('Debes seleccionar un rol.');
        return;
    }

    const payload = {
        username,
        email,
        password: password || null,       // en update, si va null no cambia
        status: chk ? chk.checked : true, // aunque el backend use solo Status internamente
        roleId
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/users' : `/api/users/${id}`;

    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando usuario...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        const text = await resp.text();
        if (!resp.ok) {
            console.error('Error guardando usuario', resp.status, text);
            showError(text || 'No se pudo guardar el usuario.');
            return;
        }

        showToast(isNew ? 'Usuario creado correctamente.' : 'Usuario actualizado correctamente.');
        userModal.hide();
        await loadUsers();
    } catch (err) {
        console.error(err);
        showError('No se pudo guardar el usuario.');
    } finally {
        hideLoading();
    }
}

// ELIMINAR (BORRADO DEFINITIVO)

async function deleteUser(id) {
    const ok = await showConfirm('¿Seguro que quieres eliminar este usuario? Esta acción no se puede deshacer.');
    if (!ok) return;

    showLoading('Eliminando usuario...');
    try {
        const resp = await authFetch(`/api/users/${id}`, { method: 'DELETE' });
        if (!resp.ok && resp.status !== 204) {
            const text = await resp.text();
            console.error('Error eliminando usuario', resp.status, text);
            showError(text || 'No se pudo eliminar el usuario.');
            return;
        }

        showToast('Usuario eliminado correctamente.');
        await loadUsers();
    } catch (err) {
        console.error(err);
        showError('No se pudo eliminar el usuario.');
    } finally {
        hideLoading();
    }
}

// EVENTOS

function attachEvents() {
    const btnNew = document.getElementById('btnNewUser');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('userForm');
    if (form) form.addEventListener('submit', saveUser);

    const tbody = document.getElementById('usersTableBody');
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
                deleteUser(id);
            }
        });
    }
}

// INICIALIZACIÓN

document.addEventListener('DOMContentLoaded', async () => {
    const modalEl = document.getElementById('userModal');
    if (modalEl && window.bootstrap) {
        userModal = new bootstrap.Modal(modalEl);
    }

    attachEvents();
    await loadRoles();
    await loadUsers();
});
