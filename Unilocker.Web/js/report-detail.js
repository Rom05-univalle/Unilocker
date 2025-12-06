import { isAuthenticated, logout } from './auth.js';
import { authFetch } from './api.js';
import { showLoading, hideLoading, showToast, showError } from './ui.js';

if (!isAuthenticated()) { window.location.href = "login.html"; }
document.getElementById("btnLogout")?.addEventListener("click", logout);

function getReportId() {
    const params = new URLSearchParams(window.location.search);
    return params.get("id") || "";
}

function formatDateTime(isoString) {
    if (!isoString) return '-';
    const d = new Date(isoString);
    if (isNaN(d)) return '-';
    return d.toLocaleString();
}

function getStatusBadge(status) {
    const badges = {
        'Pending': '<span class="badge bg-warning text-dark">Pendiente</span>',
        'InReview': '<span class="badge bg-info">En Revisión</span>',
        'Resolved': '<span class="badge bg-success">Resuelto</span>',
        'Rejected': '<span class="badge bg-danger">Rechazado</span>'
    };
    return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
}

async function loadReportDetail(id) {
    showLoading('Cargando reporte...');
    try {
        const res = await authFetch(`/api/reports/${id}`);
        if (!res.ok) {
            if (res.status === 404) {
                renderError('No se encontró el reporte.');
            } else {
                renderError('Error al cargar el reporte.');
            }
            return;
        }
        const data = await res.json();
        renderReportDetail(data);
    } catch (err) {
        console.error(err);
        renderError('Error al cargar el reporte.');
    } finally {
        hideLoading();
    }
}

function renderError(message) {
    const cont = document.getElementById("reportDetail");
    if (!cont) return;
    cont.innerHTML = `
        <div class="alert alert-danger">${message}</div>
        <a href="reports.html" class="btn btn-secondary">Volver a reportes</a>
    `;
}

function renderReportDetail(data) {
    const cont = document.getElementById("reportDetail");
    if (!cont) return;
    
    cont.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Reporte #${data.id}</h2>
            <a href="reports.html" class="btn btn-secondary">
                <i class="fa-solid fa-arrow-left me-2"></i>Volver
            </a>
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-header" style="background-color: #8B1538; color: white;">
                <h5 class="mb-0"><i class="fa-solid fa-info-circle me-2"></i>Información del Reporte</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <strong>Fecha:</strong> ${formatDateTime(data.reportDate)}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Estado:</strong> ${getStatusBadge(data.reportStatus)}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Usuario:</strong> ${data.userName ?? '-'}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Computadora:</strong> ${data.computerName ?? '-'}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Aula:</strong> ${data.classroomName ?? '-'}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Bloque:</strong> ${data.blockName ?? '-'}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Sucursal:</strong> ${data.branchName ?? '-'}
                    </div>
                    <div class="col-md-6 mb-3">
                        <strong>Tipo de Problema:</strong> ${data.problemTypeName ?? '-'}
                    </div>
                    <div class="col-12 mb-3">
                        <strong>Descripción:</strong><br>
                        ${data.description ?? '-'}
                    </div>
                    ${data.resolutionDate ? `
                    <div class="col-12 mb-3">
                        <strong>Fecha de Resolución:</strong> ${formatDateTime(data.resolutionDate)}
                    </div>
                    ` : ''}
                </div>
            </div>
        </div>

        <div class="card shadow-sm">
            <div class="card-header bg-secondary text-white">
                <h5 class="mb-0"><i class="fa-solid fa-edit me-2"></i>Actualizar Estado</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label for="newStatus" class="form-label">Nuevo Estado</label>
                        <select class="form-select" id="newStatus">
                            <option value="Pending" ${data.reportStatus === 'Pending' ? 'selected' : ''}>Pendiente</option>
                            <option value="InReview" ${data.reportStatus === 'InReview' ? 'selected' : ''}>En Revisión</option>
                            <option value="Resolved" ${data.reportStatus === 'Resolved' ? 'selected' : ''}>Resuelto</option>
                        </select>
                    </div>
                    <div class="col-md-6 d-flex align-items-end mb-3">
                        <button class="btn btn-secondary" id="btnUpdateStatus">
                            <i class="fa-solid fa-save me-2"></i>Actualizar Estado
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Agregar event listener para actualizar estado
    document.getElementById('btnUpdateStatus')?.addEventListener('click', async () => {
        await updateReportStatus(data.id);
    });
}

async function updateReportStatus(reportId) {
    const statusSelect = document.getElementById('newStatus');
    const newStatus = statusSelect?.value;
    
    if (!newStatus) {
        showError('Selecciona un estado válido');
        return;
    }

    showLoading('Actualizando estado...');
    try {
        const res = await authFetch(`/api/reports/${reportId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ reportStatus: newStatus })
        });

        if (!res.ok) {
            const errorData = await res.json();
            showError(errorData.message || 'Error al actualizar el estado');
            return;
        }

        showToast('Estado actualizado correctamente', 'success');
        // Recargar el detalle
        setTimeout(() => {
            loadReportDetail(reportId);
        }, 1000);
    } catch (err) {
        console.error(err);
        showError('Error al actualizar el estado');
    } finally {
        hideLoading();
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const id = getReportId();
    if (!id) {
        renderError('ID de reporte no proporcionado');
        return;
    }
    loadReportDetail(id);
});
