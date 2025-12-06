import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';
import { getCurrentUser } from './auth.js';

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
            <td>${u.username}</td>
            <td>${u.email}</td>
            <td>${u.phone ?? ''}</td>
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
        // Cargar usuarios
        const respUsers = await authFetch('/api/users');
        const dataUsers = await respUsers.json();

        // Cargar sesiones activas
        const respSessions = await authFetch('/api/sessions');
        const dataSessions = await respSessions.json();

        // Crear un Set con los IDs de usuarios que tienen sesiones activas
        const activeUserIds = new Set(
            dataSessions
                .filter(s => s.isActive === true)
                .map(s => s.userId)
        );

        usersCache = dataUsers.map(u => ({
            id: u.id,
            username: u.username,
            firstName: u.firstName,
            lastName: u.lastName,
            email: u.email,
            phone: u.phone,
            status: activeUserIds.has(u.id), // Estado basado en si tiene sesión activa
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
    document.getElementById('txtFirstName').value = '';
    document.getElementById('txtLastName').value = '';
    document.getElementById('txtEmail').value = '';
    document.getElementById('txtPhone').value = '';
    document.getElementById('txtPassword').value = '';

    populateRolesSelect();

    const titleEl = document.getElementById('userModalTitle');
    if (titleEl) titleEl.textContent = 'Nuevo usuario';

    userModal.show();
}

function openEditModal(id) {
    // Validación: No permitir editar el propio usuario
    const currentUser = getCurrentUser();
    if (currentUser && currentUser.userId === id) {
        showError('No puedes editar tu propia cuenta.');
        return;
    }

    const u = usersCache.find(x => x.id === id);
    if (!u) return;

    const form = document.getElementById('userForm');
    if (!form) return;

    form.dataset.id = String(u.id);

    document.getElementById('userId').value = u.id;
    document.getElementById('txtUsername').value = u.username ?? '';
    document.getElementById('txtFirstName').value = u.firstName ?? u.username ?? '';
    document.getElementById('txtLastName').value = u.lastName ?? u.username ?? '';
    document.getElementById('txtEmail').value = u.email ?? '';
    document.getElementById('txtPhone').value = u.phone ?? '';
    document.getElementById('txtPassword').value = ''; // vacío: solo cambia si escribe algo

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
    const firstName = document.getElementById('txtFirstName').value.trim();
    const lastName = document.getElementById('txtLastName').value.trim();
    const email = document.getElementById('txtEmail').value.trim();
    const phone = document.getElementById('txtPhone').value.trim();
    const password = document.getElementById('txtPassword').value;
    const selRole = document.getElementById('selUserRole');

    const roleId = selRole ? parseInt(selRole.value || '0', 10) : 0;

    if (!username) {
        showError('El username es obligatorio.');
        return;
    }
    if (!firstName) {
        showError('El nombre es obligatorio.');
        return;
    }
    if (!lastName) {
        showError('El apellido es obligatorio.');
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
        email: email || null,
        phone: phone || null,
        firstName,
        lastName,
        passwordHash: password || null,
        status: true,
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
        
        if (!resp.ok) {
            // Intentar parsear la respuesta como JSON para obtener el mensaje de error
            let errorMessage = 'No se pudo guardar el usuario.';
            try {
                const errorData = await resp.json();
                if (errorData.message) {
                    errorMessage = errorData.message;
                }
            } catch (parseError) {
                // Si no se puede parsear como JSON, intentar leer como texto
                const text = await resp.text();
                if (text) {
                    errorMessage = text;
                }
            }
            
            console.error('Error guardando usuario', resp.status, errorMessage);
            showError(errorMessage);
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
    // Obtener información del usuario actual
    const currentUser = getCurrentUser();
    if (!currentUser) {
        showError('No se pudo obtener información del usuario actual.');
        return;
    }

    // Validación 1: No permitir eliminar el propio usuario
    if (currentUser.userId === id) {
        showError('No puedes eliminar tu propia cuenta.');
        return;
    }

    // Buscar el usuario a eliminar en la caché
    const userToDelete = usersCache.find(u => u.id === id);
    if (!userToDelete) {
        showError('Usuario no encontrado.');
        return;
    }

    // Validación 2: No permitir eliminar usuarios con sesión activa
    if (userToDelete.status === true) {
        showError('No puedes eliminar un usuario con sesión activa. Debe cerrar sesión primero.');
        return;
    }

    // Validación 3: No permitir eliminar administradores (rol Admin)
    if (userToDelete.roleName && userToDelete.roleName.toLowerCase() === 'admin') {
        showError('No puedes eliminar usuarios con rol de Administrador.');
        return;
    }

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
