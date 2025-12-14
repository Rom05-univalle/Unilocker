import { authFetch } from './api.js';
import { showLoading, hideLoading } from './ui.js';

async function loadTopProblem() {
    try {
        const res = await authFetch('/api/analytics/top-problem');
        const data = await res.json();
        
        document.getElementById('topProblemName').textContent = data.problemName;
        document.getElementById('topProblemTotal').textContent = data.totalReports;
        document.getElementById('topProblemPending').textContent = data.pendingReports;
        document.getElementById('topProblemSolved').textContent = data.solvedReports;
    } catch (err) {
        console.error('Error loading top problem:', err);
    }
}

async function loadTopComputer() {
    try {
        const res = await authFetch('/api/analytics/top-failing-computer');
        const data = await res.json();
        
        document.getElementById('topComputerName').textContent = data.computerName;
        document.getElementById('topComputerClassroom').textContent = data.classroomName;
        document.getElementById('topComputerTotal').textContent = data.totalFailures;
    } catch (err) {
        console.error('Error loading top computer:', err);
    }
}

async function loadTopClassroom() {
    try {
        const res = await authFetch('/api/analytics/top-failing-classroom');
        const data = await res.json();
        
        document.getElementById('topClassroomName').textContent = data.classroomName;
        document.getElementById('topClassroomBranch').textContent = `${data.blockName} - ${data.branchName}`;
        document.getElementById('topClassroomTotal').textContent = data.totalFailures;
        document.getElementById('topClassroomComputers').textContent = data.affectedComputers;
    } catch (err) {
        console.error('Error loading top classroom:', err);
    }
}

async function loadAvgComputers() {
    try {
        const res = await authFetch('/api/analytics/average-usage-by-computer');
        const data = await res.json();
        
        const tbody = document.getElementById('avgComputerTableBody');
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">No hay datos disponibles</td></tr>';
            return;
        }
        
        tbody.innerHTML = data.map(item => `
            <tr>
                <td>${item.computerName}</td>
                <td>${item.classroomName}</td>
                <td>${item.totalSessions}</td>
                <td>${Math.round(item.averageMinutes)} min</td>
                <td>${item.totalHours.toFixed(1)} h</td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading avg computers:', err);
    }
}

async function loadAvgClassrooms() {
    try {
        const res = await authFetch('/api/analytics/average-usage-by-classroom');
        const data = await res.json();
        
        const tbody = document.getElementById('avgClassroomTableBody');
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-3">No hay datos disponibles</td></tr>';
            return;
        }
        
        tbody.innerHTML = data.map(item => `
            <tr>
                <td>${item.classroomName}</td>
                <td>${item.blockName}</td>
                <td>${item.branchName}</td>
                <td>${item.totalSessions}</td>
                <td>${item.totalComputers}</td>
                <td>${Math.round(item.averageMinutes)} min</td>
                <td>${item.totalHours.toFixed(1)} h</td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading avg classrooms:', err);
    }
}

async function loadPeakHours() {
    try {
        const res = await authFetch('/api/analytics/peak-hours');
        const data = await res.json();
        
        const chart = document.getElementById('peakHoursChart');
        if (data.length === 0) {
            chart.innerHTML = '<p class="text-center text-muted">No hay datos disponibles</p>';
            return;
        }
        
        // Crear un simple gráfico de barras con divs
        const maxSessions = Math.max(...data.map(h => h.totalSessions));
        
        chart.innerHTML = data.map(hour => {
            const percentage = (hour.totalSessions / maxSessions) * 100;
            const time = `${hour.hour}:00 - ${hour.hour + 1}:00`;
            return `
                <div class="d-flex align-items-center mb-2">
                    <div class="text-end" style="width: 120px; font-size: 0.85rem;">
                        ${time}
                    </div>
                    <div class="flex-grow-1 ms-2">
                        <div class="progress" style="height: 25px;">
                            <div class="progress-bar bg-primary" role="progressbar" 
                                 style="width: ${percentage}%" 
                                 aria-valuenow="${hour.totalSessions}" 
                                 aria-valuemin="0" 
                                 aria-valuemax="${maxSessions}">
                                ${hour.totalSessions} sesiones
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    } catch (err) {
        console.error('Error loading peak hours:', err);
    }
}

async function loadAllAnalytics() {
    showLoading('Cargando analíticas...');
    try {
        await Promise.all([
            loadTopProblem(),
            loadTopComputer(),
            loadTopClassroom(),
            loadAvgComputers(),
            loadAvgClassrooms(),
            loadPeakHours()
        ]);
    } finally {
        hideLoading();
    }
}

document.addEventListener('DOMContentLoaded', () => {
    loadAllAnalytics();
});
