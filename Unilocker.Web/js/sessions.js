import { isAuthenticated, logout } from './auth.js';
import { apiCall } from './api.js';

if (!isAuthenticated()) { window.location.href = "login.html"; }
document.getElementById("btnLogout").addEventListener("click", logout);

const statusMsg = document.getElementById("statusMsg");

function getDurationText(session) {
    if (session.durationMinutes != null) {
        const mins = session.durationMinutes;
        const h = Math.floor(mins / 60);
        const m = mins % 60;
        if (h > 0) return `${h} h ${m} min`;
        return `${m} min`;
    }
    const start = new Date(session.startDateTime);
    const now = new Date();
    const diffMs = now - start;
    const mins = Math.floor(diffMs / 60000);
    return `${mins} min`;
}

function getStateBadge(isActive) {
    const text = isActive ? "Activa" : "Finalizada";
    const color = isActive ? "success" : "secondary";
    return `<span class="badge bg-${color}">${text}</span>`;
}

function renderSessionsTable(sessions) {
    const tbody = document.querySelector("#sessionsTable tbody");
    tbody.innerHTML = "";
    sessions.forEach(sess => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${sess.id}</td>
            <td>${sess.userFullName ?? sess.userName}</td>
            <td>${sess.computerName}</td>
            <td>${sess.classroomName}</td>
            <td>${new Date(sess.startDateTime).toLocaleTimeString('es-ES',{hour:"2-digit",minute:"2-digit"})}</td>
            <td>${getDurationText(sess)}</td>
            <td>${getStateBadge(sess.isActive)}</td>
        `;
        tbody.appendChild(tr);
    });
}

async function refreshSessions(manual = false) {
    try {
        const sessions = await apiCall("/api/sessions/active", "GET");
        renderSessionsTable(sessions);
        statusMsg.textContent = manual ? "Tabla actualizada manualmente." : "Tabla actualizada automáticamente.";
    } catch {
        statusMsg.textContent = "Error al cargar sesiones.";
    }
}

document.getElementById("btnRefresh").addEventListener("click", () => refreshSessions(true));

document.addEventListener("DOMContentLoaded", () => {
    refreshSessions(true);
    setInterval(() => refreshSessions(false), 30000);
});
