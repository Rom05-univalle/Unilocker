using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Threading;
using Unilocker.Client.Helpers;
using Unilocker.Client.Models;
using Unilocker.Client.Services;
using Unilocker.Client.Views;

namespace Unilocker.Client;

public partial class MainWindow : Window
{
    private readonly AuthService _authService;
    private readonly ApiService _apiService;
    private readonly ConfigService _configService;
    private readonly SessionService _sessionService;
    private readonly ReportsService _reportsService;

    private DispatcherTimer? _durationTimer;
    private DateTime _sessionStartTime;
    private bool _isClosingBySystem = false;
    private bool _isLoggingOut = false;

    public MainWindow(AuthService authService, ApiService apiService, ConfigService configService)
    {
        InitializeComponent();

        _authService = authService;
        _apiService = apiService;
        _configService = configService;
        _sessionService = new SessionService(_apiService, _configService);
        _reportsService = new ReportsService(_apiService);

        // Iniciar sesión cuando se cargue la ventana
        Loaded += MainWindow_Loaded;

        // Verificar rol y mostrar botón de desregistro si es administrador
        CheckAdminRole();

        // Suscribirse al evento de cierre de Windows
        SystemEvents.SessionEnding += OnSystemSessionEnding;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Obtener token guardado
            string? token = _configService.GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                ModernDialog.Show(
                    "No se encontró token de autenticación. Por favor, inicia sesión nuevamente.",
                    "Error",
                    ModernDialog.DialogType.Error);
                Application.Current.Shutdown();
                return;
            }

            // Extraer userId del token JWT
            int userId = _sessionService.ExtractUserIdFromToken(token);

            // Obtener computerId almacenado
            int computerId = _sessionService.GetStoredComputerId();

            // Iniciar sesión en el servidor
            SessionResponse session;
            try
            {
                session = await _sessionService.StartSessionAsync(userId, computerId);
            }
            catch (Exception ex) when (ex.Message.Contains("409") || ex.Message.Contains("Conflict"))
            {
                var result = ModernDialog.ShowConfirm(
                    "Ya tienes una sesión activa en el sistema.\n\n" +
                    "Esto puede ocurrir si cerraste la aplicación sin cerrar sesión correctamente.\n\n" +
                    "¿Deseas cerrar tu sesión anterior y abrir una nueva?",
                    "Sesión Activa Detectada",
                    ModernDialog.DialogType.Warning);

                if (result)
                {
                    // Forzar cierre de sesiones activas del usuario
                    bool closed = await _apiService.ForceCloseUserSessionsAsync(userId);
                    
                    if (closed)
                    {
                        // Intentar iniciar sesión nuevamente
                        try
                        {
                            session = await _sessionService.StartSessionAsync(userId, computerId);
                        }
                        catch (Exception retryEx)
                        {
                            ModernDialog.Show(
                                $"Error al iniciar nueva sesión:\n\n{retryEx.Message}\n\nLa aplicación se cerrará.",
                                "Error",
                                ModernDialog.DialogType.Error);
                            Application.Current.Shutdown();
                            return;
                        }
                    }
                    else
                    {
                        ModernDialog.Show(
                            "No se pudo cerrar la sesión anterior.\n\nContacta al administrador del sistema.",
                            "Error",
                            ModernDialog.DialogType.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            // Guardar tiempo de inicio
            _sessionStartTime = session.StartDateTime;

            // Mostrar información de la sesión
            TxtWelcome.Text = $"Bienvenido, {session.UserFullName}";
            TxtUserName.Text = session.UserName;
            TxtComputerName.Text = session.ComputerName;
            TxtClassroom.Text = $"{session.ClassroomName} - {session.BlockName} - {session.BranchName}";
            TxtStartTime.Text = session.StartDateTime.ToString("dd/MM/yyyy HH:mm:ss");

            // Iniciar timer para actualizar duración
            StartDurationTimer();

            // Actualizar último heartbeat
            UpdateLastHeartbeatDisplay();

            ModernDialog.Show(
                $"✓ Sesión iniciada correctamente\n\n" +
                $"Usuario: {session.UserFullName}\n" +
                $"Computadora: {session.ComputerName}\n" +
                $"Aula: {session.ClassroomName}\n\n" +
                $"El sistema está monitoreando tu sesión.",
                "Sesión Iniciada",
                ModernDialog.DialogType.Success);
        }
        catch (Exception ex)
        {
            ModernDialog.Show(
                $"Error al iniciar sesión:\n\n{ex.Message}\n\n" +
                "La aplicación se cerrará.",
                "Error Crítico",
                ModernDialog.DialogType.Error);
            Application.Current.Shutdown();
        }
    }

    private void StartDurationTimer()
    {
        _durationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _durationTimer.Tick += (s, e) => UpdateDuration();
        _durationTimer.Start();
    }

    private void UpdateDuration()
    {
        var duration = DateTime.Now - _sessionStartTime;
        TxtDuration.Text = $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    private void UpdateLastHeartbeatDisplay()
    {
        var lastHeartbeat = _sessionService.CurrentSession?.LastHeartbeat;
        if (lastHeartbeat.HasValue)
        {
            var elapsed = DateTime.Now - lastHeartbeat.Value;
            if (elapsed.TotalSeconds < 60)
            {
                TxtLastHeartbeat.Text = "Última señal: Hace un momento";
            }
            else
            {
                TxtLastHeartbeat.Text = $"Última señal: Hace {elapsed.TotalMinutes:F0} minutos";
            }
        }
    }

    private void OnSystemSessionEnding(object sender, SessionEndingEventArgs e)
    {
        // Windows está cerrando sesión o apagando
        _isClosingBySystem = true;

        // Cancelar el cierre temporalmente para mostrar ventana de reporte
        e.Cancel = true;

        // Mostrar ventana de reporte en el thread de UI
        Dispatcher.Invoke(() =>
        {
            ShowReportWindowAndEndSession("Forced");
        });

        // Permitir que Windows continúe con el cierre
        e.Cancel = false;
    }

    private async void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = ModernDialog.ShowConfirm(
            "¿Estás seguro de que deseas cerrar tu sesión?",
            "Confirmar Cierre de Sesión",
            ModernDialog.DialogType.Question);

        if (result)
        {
            _isLoggingOut = true;
            ShowReportWindowAndEndSession("Normal");
        }
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Si es cierre del sistema, ya se manejó en OnSystemSessionEnding
        if (_isClosingBySystem)
            return;

        // Si se está cerrando sesión correctamente desde el botón, permitir
        if (_isLoggingOut)
            return;

        // MODO KIOSCO: NO permitir cerrar la ventana de ninguna forma
        // Solo se puede cerrar mediante el botón "Cerrar Sesión"
        e.Cancel = true;

        ModernDialog.Show(
            "⛔ NO PUEDES CERRAR ESTA VENTANA\n\n" +
            "Esta computadora está en modo de laboratorio.\n\n" +
            "Para cerrar tu sesión, usa el botón 'Cerrar Sesión' en la parte inferior.",
            "Acceso Restringido",
            ModernDialog.DialogType.Warning);
    }

    private async void ShowReportWindowAndEndSession(string endMethod)
    {
        try
        {
            // Detener timers
            _sessionService.StopHeartbeatTimer();
            _durationTimer?.Stop();

            if (_sessionService.CurrentSessionId.HasValue)
            {
                // Mostrar ventana de reporte (modal)
                var reportWindow = new ReportWindow(_reportsService, _sessionService.CurrentSessionId.Value);
                reportWindow.ShowDialog();

                // Finalizar sesión
                await _sessionService.EndSessionAsync(endMethod);

                ModernDialog.Show(
                    "✓ Sesión finalizada correctamente.\n\nGracias por usar Unilocker.",
                    "Sesión Finalizada",
                    ModernDialog.DialogType.Success);
            }

            // IMPORTANTE: Limpiar token para que el siguiente usuario tenga que hacer login
            _configService.ClearToken();

            // Desuscribirse del evento
            SystemEvents.SessionEnding -= OnSystemSessionEnding;

            // Cerrar aplicación
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            ModernDialog.Show(
                $"Error al finalizar sesión:\n\n{ex.Message}\n\n" +
                "La sesión se cerrará de todas formas.",
                "Error",
                ModernDialog.DialogType.Error);

            // Limpiar token incluso si hay error
            _configService.ClearToken();

            Application.Current.Shutdown();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Limpiar recursos
        _durationTimer?.Stop();
        SystemEvents.SessionEnding -= OnSystemSessionEnding;

        // CRÍTICO: Si hay una sesión activa y no estamos cerrando por logout normal,
        // intentar cerrar la sesión en la base de datos (para casos de forzar cierre)
        if (!_isLoggingOut && _sessionService.CurrentSessionId.HasValue)
        {
            try
            {
                // Intentar cerrar la sesión de manera síncrona antes de que la app termine
                var task = _sessionService.EndSessionAsync("Forced");
                task.Wait(TimeSpan.FromSeconds(2)); // Esperar máximo 2 segundos
            }
            catch (Exception ex)
            {
                // Registrar error pero no bloquear el cierre
                System.Diagnostics.Debug.WriteLine($"Error al cerrar sesión forzadamente: {ex.Message}");
            }
        }

        base.OnClosed(e);
    }

    /// <summary>
    /// Verifica si el usuario actual es administrador y muestra el botón de desregistro
    /// </summary>
    private void CheckAdminRole()
    {
        string? userRole = _configService.GetStoredUserRole();
        
        // Mostrar botón solo si el rol es "Administrador" (case-insensitive)
        if (!string.IsNullOrEmpty(userRole) && 
            userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            BtnUnregister.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Maneja el click del botón de desregistro
    /// </summary>
    private async void BtnUnregister_Click(object sender, RoutedEventArgs e)
    {
        var result = ModernDialog.ShowConfirm(
            "⚠️ ADVERTENCIA: DESREGISTRO DE COMPUTADORA ⚠️\n\n" +
            "Esta acción eliminará permanentemente:\n" +
            "• El registro de esta computadora en el sistema\n" +
            "• Todos los archivos de configuración local\n" +
            "• Los tokens de autenticación guardados\n\n" +
            "Deberá volver a registrar esta computadora desde cero.\n\n" +
            "¿Está completamente seguro que desea continuar?",
            "Confirmar Desregistro de Computadora",
            ModernDialog.DialogType.Warning);

        if (result)
        {
            // Confirmar nuevamente
            var confirmResult = ModernDialog.ShowConfirm(
                "Esta es su última oportunidad para cancelar.\n\n" +
                "¿Realmente desea desregistrar esta computadora?",
                "Confirmación Final",
                ModernDialog.DialogType.Error);

            if (confirmResult)
            {
                // Intentar desregistrar en el servidor primero
                var computerId = _configService.GetStoredComputerId();
                if (computerId.HasValue)
                {
                    try
                    {
                        await _apiService.UnregisterComputerAsync(computerId.Value);
                    }
                    catch
                    {
                        // Si falla la llamada al API, continuar con el desregistro local
                        // (por ejemplo, si no hay conexión)
                    }
                }

                if (_configService.UnregisterComputer())
                {
                    ModernDialog.Show(
                        "✓ Computadora desregistrada exitosamente.\n\n" +
                        "Todos los archivos de configuración han sido eliminados.\n\n" +
                        "La aplicación se cerrará ahora.\n\n" +
                        "Deberá ejecutar el proceso de registro nuevamente.",
                        "Desregistro Exitoso",
                        ModernDialog.DialogType.Success);

                    // Detener timers
                    _durationTimer?.Stop();
                    _sessionService.StopHeartbeatTimer();

                    // Cerrar sin mostrar reportes ni finalizar sesión
                    _isClosingBySystem = true;
                    Application.Current.Shutdown();
                }
                else
                {
                    ModernDialog.Show(
                        "❌ Error al desregistrar la computadora.\n\n" +
                        "No se pudieron eliminar algunos archivos de configuración.\n\n" +
                        "Por favor, contacte al administrador del sistema.",
                        "Error de Desregistro",
                        ModernDialog.DialogType.Error);
                }
            }
        }
    }
}