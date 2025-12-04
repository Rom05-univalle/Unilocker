// js/dashboard.js

// Requiere que api.js defina API_BASE_URL y authFetch en global.

// ----------------- Estado de filtros -----------------

let currentStartDate = null;
let currentEndDate = null;

function buildDateQuery() {
  const params = new URLSearchParams();
  if (currentStartDate) params.append('startDate', currentStartDate);
  if (currentEndDate) params.append('endDate', currentEndDate);
  const qs = params.toString();
  return qs ? `?${qs}` : '';
}

// ----------------- Dashboard stats numéricos -----------------

async function loadDashboardStats() {
  try {
    const qs = buildDateQuery();
    const res = await authFetch(`${API_BASE_URL}/api/dashboard/stats${qs}`);
    const data = await res.json();

    document.getElementById('totalUsers')?.innerText = data.totalUsers ?? 0;
    document.getElementById('totalSessions')?.innerText = data.totalSessions ?? 0;
    document.getElementById('totalReports')?.innerText = data.totalReports ?? 0;
    document.getElementById('activeComputers')?.innerText = data.activeComputers ?? 0;
  } catch (err) {
    console.error('Error cargando stats de dashboard', err);
  }
}

// ----------------- Chart.js helpers -----------------

let sessionsChartInstance;
let reportsChartInstance;
let topComputersChartInstance;
let topUsersChartInstance;

function destroyIfExists(chart) {
  if (chart) {
    chart.destroy();
  }
}

const palette = {
  primary: '#82284b',
  accent: '#9facb5',
  success: '#28a745',
  warning: '#ffc107',
  info: '#17a2b8',
  danger: '#dc3545',
  purple: '#6f42c1',
  cyan: '#20c997'
};

function getPieColors(count) {
  const base = [
    palette.primary,
    palette.accent,
    palette.success,
    palette.warning,
    palette.info,
    palette.purple,
    palette.cyan,
    palette.danger
  ];
  const colors = [];
  for (let i = 0; i < count; i++) {
    colors.push(base[i % base.length]);
  }
  return colors;
}

// ----------------- Gráfico: Sesiones por día -----------------

async function loadSessionsChart() {
  try {
    const qs = buildDateQuery();
    const res = await authFetch(`${API_BASE_URL}/api/stats/sessions-by-day${qs}`);
    const data = await res.json(); // [{ date, count }, ...]

    const labels = data.map(x => x.date);
    const values = data.map(x => x.count);

    const canvas = document.getElementById('sessionsChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    destroyIfExists(sessionsChartInstance);

    sessionsChartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Sesiones',
          data: values,
          borderColor: palette.primary,
          backgroundColor: 'rgba(130, 40, 75, 0.15)',
          tension: 0.3,
          fill: true,
          pointRadius: 4,
          pointBackgroundColor: palette.primary
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'top'
          },
          tooltip: {
            callbacks: {
              label: (ctx) => `Sesiones: ${ctx.parsed.y}`
            }
          }
        },
        scales: {
          x: {
            title: {
              display: true,
              text: 'Fecha'
            }
          },
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Cantidad'
            }
          }
        }
      }
    });
  } catch (err) {
    console.error('Error cargando sesiones por día', err);
  }
}

// ----------------- Gráfico: Reportes por tipo -----------------

async function loadReportsChart() {
  try {
    const qs = buildDateQuery();
    const res = await authFetch(`${API_BASE_URL}/api/stats/reports-by-problem${qs}`);
    const data = await res.json(); // [{ problemType, count }, ...]

    const labels = data.map(x => x.problemType);
    const values = data.map(x => x.count);
    const colors = getPieColors(values.length);

    const canvas = document.getElementById('reportsChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    destroyIfExists(reportsChartInstance);

    reportsChartInstance = new Chart(ctx, {
      type: 'pie',
      data: {
        labels,
        datasets: [{
          data: values,
          backgroundColor: colors,
          borderColor: '#ffffff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom'
          },
          tooltip: {
            callbacks: {
              label: (ctx) => {
                const label = ctx.label || '';
                const value = ctx.parsed || 0;
                const total = ctx.chart._metasets[0].total;
                const perc = total ? ((value / total) * 100).toFixed(1) : 0;
                return `${label}: ${value} (${perc}%)`;
              }
            }
          }
        }
      }
    });
  } catch (err) {
    console.error('Error cargando reportes por tipo', err);
  }
}

// ----------------- Gráfico: Top equipos -----------------

async function loadTopComputersChart() {
  try {
    const qs = buildDateQuery();
    const res = await authFetch(`${API_BASE_URL}/api/stats/top-computers${qs}`);
    const data = await res.json(); // [{ computerName, reportCount }, ...]

    const labels = data.map(x => x.computerName);
    const values = data.map(x => x.reportCount);

    const canvas = document.getElementById('topComputersChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    destroyIfExists(topComputersChartInstance);

    topComputersChartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Reportes',
          data: values,
          backgroundColor: 'rgba(130, 40, 75, 0.7)',
          borderColor: palette.primary,
          borderWidth: 1
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (ctx) => `Reportes: ${ctx.parsed.x}`
            }
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Cantidad de reportes'
            }
          },
          y: {
            title: {
              display: true,
              text: 'Equipo'
            }
          }
        }
      }
    });
  } catch (err) {
    console.error('Error cargando top equipos', err);
  }
}

// ----------------- Gráfico: Top usuarios -----------------

async function loadTopUsersChart() {
  try {
    const qs = buildDateQuery();
    const res = await authFetch(`${API_BASE_URL}/api/stats/top-users${qs}`);
    const data = await res.json(); // [{ username, fullName, totalMinutes }, ...]

    const labels = data.map(x => x.fullName || x.username);
    const values = data.map(x => x.totalMinutes);

    const canvas = document.getElementById('topUsersChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    destroyIfExists(topUsersChartInstance);

    topUsersChartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Minutos de uso',
          data: values,
          backgroundColor: 'rgba(39, 174, 96, 0.7)',
          borderColor: '#27ae60',
          borderWidth: 1
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (ctx) => `Minutos: ${ctx.parsed.x}`
            }
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Minutos totales'
            }
          },
          y: {
            title: {
              display: true,
              text: 'Usuario'
            }
          }
        }
      }
    });
  } catch (err) {
    console.error('Error cargando top usuarios', err);
  }
}

// ----------------- Exportar CSV -----------------

async function exportToCSV() {
  try {
    const qs = buildDateQuery();

    const [
      sessionsRes,
      reportsRes,
      topComputersRes,
      topUsersRes
    ] = await Promise.all([
      authFetch(`${API_BASE_URL}/api/stats/sessions-by-day${qs}`),
      authFetch(`${API_BASE_URL}/api/stats/reports-by-problem${qs}`),
      authFetch(`${API_BASE_URL}/api/stats/top-computers${qs}`),
      authFetch(`${API_BASE_URL}/api/stats/top-users${qs}`)
    ]);

    const sessions = await sessionsRes.json();
    const reports = await reportsRes.json();
    const computers = await topComputersRes.json();
    const users = await topUsersRes.json();

    let csv = '';

    csv += 'Sesiones por día\n';
    csv += 'Fecha,Cantidad\n';
    sessions.forEach(row => {
      csv += `${row.date},${row.count}\n`;
    });
    csv += '\n';

    csv += 'Reportes por tipo de problema\n';
    csv += 'Tipo,Cantidad\n';
    reports.forEach(row => {
      csv += `${row.problemType},${row.count}\n`;
    });
    csv += '\n';

    csv += 'Top 5 equipos con más reportes\n';
    csv += 'Equipo,Reportes\n';
    computers.forEach(row => {
      csv += `${row.computerName},${row.reportCount}\n`;
    });
    csv += '\n';

    csv += 'Top 5 usuarios más activos\n';
    csv += 'Usuario,MinutosTotales\n';
    users.forEach(row => {
      const name = row.fullName || row.username;
      csv += `${name},${row.totalMinutes}\n`;
    });

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    const fileName = `unilocker-dashboard-${new Date().toISOString().slice(0, 10)}.csv`;

    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  } catch (err) {
    console.error('Error exportando CSV', err);
  }
}

// ----------------- Filtros y botones -----------------

function setupFilters() {
  const startInput = document.getElementById('startDate');
  const endInput = document.getElementById('endDate');
  const filterBtn = document.getElementById('filterBtn');

  if (!startInput || !endInput || !filterBtn) return;

  filterBtn.addEventListener('click', async () => {
    currentStartDate = startInput.value || null;
    currentEndDate = endInput.value || null;

    try {
      await loadDashboardStats();
      await loadSessionsChart();
      await loadReportsChart();
      await loadTopComputersChart();
      await loadTopUsersChart();
    } catch (err) {
      console.error('Error al recargar con filtros', err);
    }
  });
}

function setupExport() {
  const exportBtn = document.getElementById('exportBtn');
  if (!exportBtn) return;
  exportBtn.addEventListener('click', exportToCSV);
}

// ----------------- Init -----------------

async function initDashboard() {
  try {
    setupFilters();
    setupExport();
    await loadDashboardStats();
    await loadSessionsChart();
    await loadReportsChart();
    await loadTopComputersChart();
    await loadTopUsersChart();
  } catch (err) {
    console.error('Error inicializando dashboard', err);
  }
}

document.addEventListener('DOMContentLoaded', initDashboard);
