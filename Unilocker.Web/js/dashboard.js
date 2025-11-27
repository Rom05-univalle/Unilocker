import { isAuthenticated, logout } from './auth.js';
import { apiCall } from './api.js';

if (!isAuthenticated()) { window.location.href = "login.html"; }

document.getElementById("btnLogout").addEventListener("click", logout);
document.getElementById("btnViewActiveSessions").onclick = () => window.location.href = "sessions.html";
document.getElementById("btnViewPendingReports").onclick = () => window.location.href = "reports.html";

async function loadDashboardStats() {
    try {
        const data = await apiCall("/api/dashboard/stats", "GET");
        document.getElementById("statTotalSessionsToday").textContent = data.totalSessionsToday ?? "-";
        document.getElementById("statActiveSessions").textContent = data.activeSessions ?? "-";
        document.getElementById("statPendingReports").textContent = data.pendingReports ?? "-";
        document.getElementById("statRegisteredComputers").textContent = data.registeredComputers ?? "-";
    } catch {
        
    }
}

document.addEventListener("DOMContentLoaded", loadDashboardStats);
