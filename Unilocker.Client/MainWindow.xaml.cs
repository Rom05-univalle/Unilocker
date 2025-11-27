using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Threading;
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
                MessageBox.Show("No se encontró token de autenticación. Por favor, inicia sesión nuevamente.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var result = MessageBox.Show(
                    "Ya tienes una sesión activa en el sistema.\n\n" +
                    "Esto puede ocurrir si cerraste la aplicación sin cerrar sesión correctamente.\n\n" +
                    "¿Deseas que el administrador cierre tu sesión anterior?",
                    "Sesión Activa Detectada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Application.Current.Shutdown();
                return;
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

            MessageBox.Show(
                $"✓ Sesión iniciada correctamente\n\n" +
                $"Usuario: {session.UserFullName}\n" +
                $"Computadora: {session.ComputerName}\n" +
                $"Aula: {session.ClassroomName}\n\n" +
                $"El sistema está monitoreando tu sesión.",
                "Sesión Iniciada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al iniciar sesión:\n\n{ex.Message}\n\n" +
                "La aplicación se cerrará.",
                "Error Crítico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
        var result = MessageBox.Show(
            "¿Estás seguro de que deseas cerrar tu sesión?",
            "Confirmar Cierre de Sesión",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ShowReportWindowAndEndSession("Normal");
        }
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Si es cierre del sistema, ya se manejó en OnSystemSessionEnding
        if (_isClosingBySystem)
            return;

        // Si el usuario intenta cerrar la ventana con X
        e.Cancel = true;

        var result = MessageBox.Show(
            "No debes cerrar esta ventana directamente.\n\n" +
            "¿Deseas cerrar tu sesión correctamente?",
            "Advertencia",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            e.Cancel = false;
            ShowReportWindowAndEndSession("Normal");
        }
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

                MessageBox.Show(
                    "✓ Sesión finalizada correctamente.\n\nGracias por usar Unilocker.",
                    "Sesión Finalizada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            // Desuscribirse del evento
            SystemEvents.SessionEnding -= OnSystemSessionEnding;

            // Cerrar aplicación
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al finalizar sesión:\n\n{ex.Message}\n\n" +
                "La sesión se cerrará de todas formas.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Limpiar recursos
        _durationTimer?.Stop();
        SystemEvents.SessionEnding -= OnSystemSessionEnding;
        base.OnClosed(e);
    }
}