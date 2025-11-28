import { isAuthenticated, logout } from './auth.js';
import { apiCall } from './api.js';

if (!isAuthenticated()) { window.location.href = "login.html"; }
document.getElementById("btnLogout").addEventListener("click", logout);

let problemTypes = [];
let reports = [];

function txtEstado(status) {
    if (status === "Pending") return "Pendiente";
    if (status === "InReview") return "En revisión";
    if (status === "Resolved") return "Resuelto";
    if (status === "Rejected") return "Rechazado";
    return status;
}

function statusBadge(status) {
    let color = "secondary";
    if (status === "Pending") color = "warning";
    else if (status === "InReview") color = "info";
    else if (status === "Resolved") color = "success";
    else if (status === "Rejected") color = "danger";
    return `<span class="badge bg-${color}">${txtEstado(status)}</span>`;
}

function renderTypes() {
    const sel = document.getElementById("filterType");
    sel.innerHTML = `<option value="">Todos</option>`;
    problemTypes.forEach(t => {
        sel.innerHTML += `<option value="${t.id}">${t.name}</option>`;
    });
}

function renderReportsTable(list) {
    const tbody = document.querySelector("#reportsTable tbody");
    tbody.innerHTML = "";
    list.forEach(r => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${r.id}</td>
            <td>${r.userFullName ?? r.userName}</td>
            <td>${r.computerName}</td>
            <td>${r.problemTypeName}</td>
            <td>${r.description}</td>
            <td>${new Date(r.reportDate).toLocaleString('es-ES',{dateStyle:"short",timeStyle:"short"})}</td>
            <td>${statusBadge(r.reportStatus)}</td>
            <td>
                <button class="btn btn-info btn-sm" onclick="window.showReportDetail(${r.id})">
                    Ver Detalle
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function applyFilters() {
    const status = document.getElementById("filterStatus").value;
    const typeId = document.getElementById("filterType").value;
    let filtered = [...reports];
    if (status) filtered = filtered.filter(r => r.reportStatus === status);
    if (typeId) filtered = filtered.filter(r => r.problemTypeId == typeId);
    renderReportsTable(filtered);
}

window.showReportDetail = async function(id) {
    try {
        const rep = await apiCall(`/api/reports/${id}`, "GET");
        const modalBody = document.getElementById("modalBodyReporte");
        modalBody.innerHTML = `
            <strong>ID:</strong> ${rep.id}<br>
            <strong>Usuario:</strong> ${rep.userFullName ?? rep.userName}<br>
            <strong>Computadora:</strong> ${rep.computerName}<br>
            <strong>Estado:</strong> ${txtEstado(rep.reportStatus)}<br>
            <strong>Tipo problema:</strong> ${rep.problemTypeName}<br>
            <strong>Descripción:</strong> ${rep.description}<br>
            <strong>Fecha:</strong> ${new Date(rep.reportDate).toLocaleString('es-ES',{dateStyle:"short",timeStyle:"short"})}<br>
        `;
        const btn = document.getElementById("btnMarcarResuelto");
        btn.style.display = rep.reportStatus !== "Resolved" ? "inline-block" : "none";
        btn.onclick = async function() {
            await apiCall(`/api/reports/${id}/status`, "PUT", { reportStatus: "Resolved" });
            await loadReports();
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalReporte'));
            modal.hide();
        };
        const modal = new bootstrap.Modal(document.getElementById('modalReporte'));
        modal.show();
    } catch {
       
    }
};

async function loadProblemTypes() {
    problemTypes = await apiCall("/api/problemtypes", "GET");
    renderTypes();
}

async function loadReports() {
    const status = document.getElementById("filterStatus").value;
    const typeId = document.getElementById("filterType").value;

    const params = new URLSearchParams();
    if (status) params.append("status", status);
    if (typeId) params.append("problemTypeId", typeId);

    const query = params.toString() ? `?${params.toString()}` : "";
    reports = await apiCall(`/api/reports${query}`, "GET");
    applyFilters();
}

document.getElementById("filtersForm").addEventListener("submit", function(e) {
    e.preventDefault();
    loadReports();
});

document.addEventListener("DOMContentLoaded", async () => {
    try {
        await loadProblemTypes();
        await loadReports();
    } catch {
       
    }
});
