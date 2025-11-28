import { isAuthenticated, logout } from './auth.js';
if (!isAuthenticated()) { window.location.href = "login.html"; }
document.getElementById("btnLogout").addEventListener("click", logout);

function getReportId() {
    const params = new URLSearchParams(window.location.search);
    return params.get("id") || "";
}

const reports = {
    "201": {id:201,computer:"PC-02",classroom:"Aula 1",desc:"No enciende",status:"Pendiente",details:"Revisar fuente y cables."},
    "202": {id:202,computer:"PC-15",classroom:"Aula 2",desc:"No reconoce mouse",status:"Resuelto",details:"Cambiado el mouse."}
};

function renderReportDetail(id) {
    const data = reports[id];
    const cont = document.getElementById("reportDetail");
    if (!data) {
        cont.innerHTML = `<div class="alert alert-danger">No se encontró el reporte.</div>`;
        return;
    }
    cont.innerHTML = `
        <h2>Reporte ID ${data.id}</h2>
        <ul class="list-group mb-3">
            <li class="list-group-item"><strong>Equipo:</strong> ${data.computer}</li>
            <li class="list-group-item"><strong>Aula:</strong> ${data.classroom}</li>
            <li class="list-group-item"><strong>Descripción:</strong> ${data.desc}</li>
            <li class="list-group-item"><strong>Estado:</strong> ${data.status}</li>
            <li class="list-group-item"><strong>Detalles:</strong> ${data.details}</li>
        </ul>
        <a href="reports.html" class="btn btn-secondary">Volver a reportes</a>
    `;
}

document.addEventListener("DOMContentLoaded", () => {
    const id = getReportId();
    renderReportDetail(id);
});
