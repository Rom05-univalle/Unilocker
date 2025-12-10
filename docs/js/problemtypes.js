import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError, showConfirm } from './ui.js';

let problemTypeModal;
let problemTypesCache = [];

// RENDER TABLA

function renderProblemTypes(items) {
    const tbody = document.getElementById('problemTypesTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    items.forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${p.name}</td>
            <td>${p.description || '<em class="text-muted">Sin descripción</em>'}</td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${p.id}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-danger btn-delete" data-id="${p.id}">
                    Eliminar
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// CARGA TIPOS DE PROBLEMA

async function loadProblemTypes() {
    showLoading('Cargando tipos de problema...');
    try {
        const resp = await authFetch('/api/problemtypes');
        const data = await resp.json();

        problemTypesCache = data.map(p => ({
            id: p.id,
            name: p.name,
            description: p.description,
            status: p.status === true || p.status === 1
        }));

        renderProblemTypes(problemTypesCache);
    } catch (err) {
        console.error(err);
        showError('Error al cargar tipos de problema.');
    } finally {
        hideLoading();
    }
}

// MODAL CREAR / EDITAR

function openCreateModal() {
    const form = document.getElementById('problemTypeForm');
    if (!form) return;

    form.reset();
    form.dataset.id = '';

    document.getElementById('problemTypeId').value = '';
    document.getElementById('txtProblemTypeName').value = '';
    document.getElementById('txtDescription').value = '';

    const titleEl = document.getElementById('problemTypeModalTitle');
    if (titleEl) titleEl.textContent = 'Nuevo tipo de problema';

    problemTypeModal.show();
}

function openEditModal(id) {
    const p = problemTypesCache.find(x => x.id === id);
    if (!p) return;

    const form = document.getElementById('problemTypeForm');
    if (!form) return;

    form.dataset.id = String(p.id);

    document.getElementById('problemTypeId').value = p.id;
    document.getElementById('txtProblemTypeName').value = p.name ?? '';
    document.getElementById('txtDescription').value = p.description ?? '';

    const titleEl = document.getElementById('problemTypeModalTitle');
    if (titleEl) titleEl.textContent = 'Editar tipo de problema';

    problemTypeModal.show();
}

// GUARDAR (CREATE / UPDATE)

async function saveProblemType(e) {
    e.preventDefault();

    const form = document.getElementById('problemTypeForm');
    const id = form.dataset.id;

    const name = document.getElementById('txtProblemTypeName').value.trim();
    const description = document.getElementById('txtDescription').value.trim();
    if (!name) {
        showError('El nombre es obligatorio.');
        return;
    }

    const payload = {
        name,
        description: description || null,
        status: true
    };

    const isNew = !id;
    const method = isNew ? 'POST' : 'PUT';
    const url = isNew ? '/api/problemtypes' : `/api/problemtypes/${id}`;

    // Para PUT, agregar el id al payload
    if (!isNew) {
        payload.id = parseInt(id, 10);
    }

    showLoading('Guardando tipo de problema...');
    try {
        const resp = await authFetch(url, { method, body: payload });
        const data = await resp.json();

        showToast(data.message || (isNew ? 'Tipo de problema creado correctamente.' : 'Tipo de problema actualizado correctamente.'));
        problemTypeModal.hide();
        await loadProblemTypes();
    } catch (err) {
        console.error(err);
        showError(err.message || 'No se pudo guardar el tipo de problema.');
    } finally {
        hideLoading();
    }
}

// ELIMINAR

async function deleteProblemType(id) {
    const ok = await showConfirm('¿Seguro que quieres eliminar este tipo de problema?');
    if (!ok) return;

    showLoading('Eliminando tipo de problema...');
    try {
        const resp = await authFetch(`/api/problemtypes/${id}`, { method: 'DELETE' });
        const data = await resp.json();

        showToast(data.message || 'Tipo de problema eliminado correctamente.');
        await loadProblemTypes();
    } catch (err) {
        console.error(err);
        showError(err.message || 'No se pudo eliminar el tipo de problema.');
    } finally {
        hideLoading();
    }
}

// EVENTOS

function attachEvents() {
    const btnNew = document.getElementById('btnNewProblemType');
    if (btnNew) btnNew.addEventListener('click', openCreateModal);

    const form = document.getElementById('problemTypeForm');
    if (form) form.addEventListener('submit', saveProblemType);

    const tbody = document.getElementById('problemTypesTableBody');
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
                deleteProblemType(id);
            }
        });
    }
}

// INICIALIZACIÓN

document.addEventListener('DOMContentLoaded', async () => {
    const modalEl = document.getElementById('problemTypeModal');
    if (modalEl && window.bootstrap) {
        problemTypeModal = new bootstrap.Modal(modalEl);
    }

    attachEvents();
    await loadProblemTypes();
});
