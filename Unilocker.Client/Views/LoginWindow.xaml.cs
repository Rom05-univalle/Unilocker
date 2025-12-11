using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Unilocker.Client.Helpers;
using Unilocker.Client.Services;

namespace Unilocker.Client.Views;

public partial class LoginWindow : Window
{
    private readonly ConfigService _configService;
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly HardwareService _hardwareService;

    // Variables para el flujo 2FA
    private int _currentUserId;
    private DispatcherTimer? _countdownTimer;
    private int _remainingSeconds = 600; // 10 minutos
    
    // Variable para permitir cierre solo cuando se completa login
    private bool _allowClose = false;
    
    // Variable para permitir cierre cuando hay problemas de conexión
    private bool _hasConnectionIssue = false;

    public LoginWindow()
    {
        InitializeComponent();

        // Inicializar servicios
        _configService = new ConfigService();
        _apiService = new ApiService(_configService.GetApiBaseUrl());
        _authService = new AuthService(_apiService, _configService);
        _hardwareService = new HardwareService();

        // Mostrar información de la computadora
        ShowComputerInfo();

        // Focus en el campo de usuario
        Loaded += async (s, e) => 
        {
            TxtUsername.Focus();
            // Verificar conexión al inicio
            await CheckInitialConnectionAsync();
        };
    }

    private async System.Threading.Tasks.Task CheckInitialConnectionAsync()
    {
        try
        {
            bool isConnected = await _apiService.TestConnectionAsync();
            
            if (!isConnected)
            {
                _hasConnectionIssue = true;
                ShowError("⚠️ No se puede conectar con el servidor. Presiona Alt+F4 para cerrar si necesitas salir.");
            }
        }
        catch
        {
            _hasConnectionIssue = true;
            ShowError("⚠️ Error al verificar conexión con el servidor. Presiona Alt+F4 para cerrar si necesitas salir.");
        }
    }

    private void ShowComputerInfo()
    {
        try
        {
            var hardwareInfo = _hardwareService.GetHardwareInfo();
            Guid machineId = _configService.GetOrCreateMachineId();

            // Intentar obtener el nombre registrado del equipo
            string computerDisplayName = hardwareInfo.ComputerName;
            
            // Si el equipo está registrado, mostrar el nombre personalizado guardado
            if (_configService.IsComputerRegistered())
            {
                string? savedComputerName = _configService.GetStoredComputerName();
                if (!string.IsNullOrEmpty(savedComputerName))
                {
                    computerDisplayName = savedComputerName;
                }
            }

            TxtComputerInfo.Text = $"Equipo: {computerDisplayName} | Marca: {hardwareInfo.Manufacturer ?? "N/A"} | ID: {machineId.ToString().Substring(0, 8)}...";
        }
        catch
        {
            TxtComputerInfo.Text = "No se pudo obtener información del equipo";
        }
    }

    // ========== PANEL 1: LOGIN ==========

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        await PerformLoginAsync();
    }

    private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Pasar al campo de contraseña
            TxtPassword.Focus();
        }
    }

    private async void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformLoginAsync();
        }
    }

    private async void TxtPasswordVisible_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformLoginAsync();
        }
    }

    private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
    {
        if (TxtPassword.Visibility == Visibility.Visible)
        {
            // Mostrar contraseña
            TxtPasswordVisible.Text = TxtPassword.Password;
            TxtPassword.Visibility = Visibility.Collapsed;
            TxtPasswordVisible.Visibility = Visibility.Visible;
            TxtPasswordIcon.Text = "👁‍🗨"; // Ojo cerrado
            TxtPasswordVisible.Focus();
            TxtPasswordVisible.CaretIndex = TxtPasswordVisible.Text.Length;
        }
        else
        {
            // Ocultar contraseña
            TxtPassword.Password = TxtPasswordVisible.Text;
            TxtPasswordVisible.Visibility = Visibility.Collapsed;
            TxtPassword.Visibility = Visibility.Visible;
            TxtPasswordIcon.Text = "👁"; // Ojo abierto
            TxtPassword.Focus();
        }
    }

    private async System.Threading.Tasks.Task PerformLoginAsync()
    {
        // Validar campos
        string username = TxtUsername.Text.Trim();
        string password = TxtPassword.Visibility == Visibility.Visible 
            ? TxtPassword.Password 
            : TxtPasswordVisible.Text;

        if (string.IsNullOrEmpty(username))
        {
            ShowError("Por favor ingrese su usuario");
            TxtUsername.Focus();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Por favor ingrese su contraseña");
            TxtPassword.Focus();
            return;
        }

        // Deshabilitar controles
        SetLoadingState(true);
        HideError();

        try
        {
            // Intentar login
            var loginResponse = await _authService.LoginAsync(username, password);

            // Verificar si requiere 2FA
            if (loginResponse.RequiresVerification)
            {
                // Cambiar al panel de verificación
                _currentUserId = loginResponse.UserId;
                ShowVerificationPanel(loginResponse.MaskedEmail ?? "***@***.com");
            }
            else
            {
                // Login exitoso directo (sin 2FA)
                ShowSuccess($"Bienvenido, {loginResponse.Username}!");
                await System.Threading.Tasks.Task.Delay(800);
                OpenMainWindow();
            }
        }
        catch (Exception ex)
        {
            // Detectar errores de conexión vs errores de autenticación
            if (ex.Message.Contains("No se pudo conectar con el servidor"))
            {
                // Error de conexión con el servidor
                _hasConnectionIssue = true;
                ShowError($"⚠️ {ex.Message}\n\nPresiona Alt+F4 para cerrar si necesitas salir.");
            }
            else
            {
                // Error de autenticación (usuario no existe, contraseña incorrecta, usuario inactivo/bloqueado)
                ShowError(ex.Message);
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        BtnLogin.IsEnabled = !isLoading;
        TxtUsername.IsEnabled = !isLoading;
        TxtPassword.IsEnabled = !isLoading;
        ProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

        if (isLoading)
        {
            BtnLogin.Content = "INICIANDO SESIÓN...";
        }
        else
        {
            BtnLogin.Content = "INICIAR SESIÓN";
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Foreground = System.Windows.Media.Brushes.Red;
        ErrorBorder.Visibility = Visibility.Visible; // ← CAMBIO AQUÍ
    }

    private void HideError()
    {
        TxtError.Text = "";
        ErrorBorder.Visibility = Visibility.Collapsed; // ← CAMBIO AQUÍ
    }

    private void ShowSuccess(string message)
    {
        TxtError.Text = message;
        TxtError.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(76, 175, 80)); // Verde

        ErrorBorder.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(232, 245, 233)); // Verde claro

        ErrorBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(76, 175, 80)); // Borde verde

        ErrorBorder.Visibility = Visibility.Visible; // ← CAMBIO AQUÍ
    }

    // ========== PANEL 2: VERIFICACIÓN 2FA ==========

    private void ShowVerificationPanel(string maskedEmail)
    {
        // Ocultar panel de login
        PanelLogin.Visibility = Visibility.Collapsed;

        // Mostrar panel de verificación
        PanelVerification.Visibility = Visibility.Visible;

        // Mostrar email enmascarado
        TxtMaskedEmail.Text = maskedEmail;

        // Limpiar campo de código
        TxtVerificationCode.Text = "";
        TxtVerificationCode.Focus();

        // Iniciar countdown timer
        StartCountdownTimer();
    }

    private void StartCountdownTimer()
    {
        _remainingSeconds = 600; // 10 minutos

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _countdownTimer.Tick += (s, e) =>
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                _countdownTimer?.Stop();
                ShowErrorVerification("El código ha expirado. Por favor, vuelve a iniciar sesión.");
                BtnVerify.IsEnabled = false;
                TxtVerificationCode.IsEnabled = false;
            }
            else
            {
                int minutes = _remainingSeconds / 60;
                int seconds = _remainingSeconds % 60;
                TxtTimer.Text = $"⏱️ Código válido por {minutes}:{seconds:D2}";

                // Cambiar color si quedan menos de 2 minutos
                if (_remainingSeconds < 120)
                {
                    TxtTimer.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        };

        _countdownTimer.Start();
    }

    private async void BtnVerify_Click(object sender, RoutedEventArgs e)
    {
        await PerformVerificationAsync();
    }

    private async void TxtVerificationCode_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && TxtVerificationCode.Text.Length == 6)
        {
            await PerformVerificationAsync();
        }
    }

    private void TxtVerificationCode_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Solo permitir números
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private async System.Threading.Tasks.Task PerformVerificationAsync()
    {
        string code = TxtVerificationCode.Text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            ShowErrorVerification("Por favor ingrese el código");
            TxtVerificationCode.Focus();
            return;
        }

        if (code.Length != 6)
        {
            ShowErrorVerification("El código debe tener 6 dígitos");
            TxtVerificationCode.Focus();
            return;
        }

        SetLoadingStateVerification(true);
        HideErrorVerification();

        try
        {
            // Verificar código
            var loginResponse = await _authService.VerifyCodeAsync(_currentUserId, code);

            // Detener timer
            _countdownTimer?.Stop();

            // Login exitoso
            ShowSuccessVerification($"✅ Código verificado correctamente");
            await System.Threading.Tasks.Task.Delay(800);
            OpenMainWindow();
        }
        catch (Exception ex)
        {
            // Detectar errores de conexión vs errores de verificación
            if (ex.Message.Contains("No se pudo conectar con el servidor"))
            {
                // Error de conexión con el servidor
                _hasConnectionIssue = true;
                ShowErrorVerification($"⚠️ {ex.Message}\n\nPresiona Alt+F4 para cerrar si necesitas salir.");
            }
            else
            {
                // Error de verificación (código incorrecto, expirado, etc.)
                ShowErrorVerification(ex.Message);
            }
            
            TxtVerificationCode.SelectAll();
            TxtVerificationCode.Focus();
        }
        finally
        {
            SetLoadingStateVerification(false);
        }
    }

    private void SetLoadingStateVerification(bool isLoading)
    {
        BtnVerify.IsEnabled = !isLoading;
        TxtVerificationCode.IsEnabled = !isLoading;
        BtnBack.IsEnabled = !isLoading;
        ProgressBarVerification.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

        if (isLoading)
        {
            BtnVerify.Content = "VERIFICANDO...";
        }
        else
        {
            BtnVerify.Content = "VERIFICAR CÓDIGO";
        }
    }

    private void ShowErrorVerification(string message)
    {
        TxtErrorVerification.Text = message;
        TxtErrorVerification.Foreground = System.Windows.Media.Brushes.Red;
        ErrorBorderVerification.Visibility = Visibility.Visible; // ← CAMBIO AQUÍ
    }

    private void HideErrorVerification()
    {
        TxtErrorVerification.Text = "";
        ErrorBorderVerification.Visibility = Visibility.Collapsed; // ← CAMBIO AQUÍ
    }

    private void ShowSuccessVerification(string message)
    {
        TxtErrorVerification.Text = message;
        TxtErrorVerification.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(76, 175, 80)); // Verde

        ErrorBorderVerification.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(232, 245, 233)); // Verde claro

        ErrorBorderVerification.BorderBrush = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(76, 175, 80)); // Borde verde

        ErrorBorderVerification.Visibility = Visibility.Visible; // ← CAMBIO AQUÍ
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        // Detener timer
        _countdownTimer?.Stop();

        // Volver al panel de login
        PanelVerification.Visibility = Visibility.Collapsed;
        PanelLogin.Visibility = Visibility.Visible;

        // Limpiar campos
        TxtPassword.Clear();
        TxtVerificationCode.Clear();
        HideError();
        HideErrorVerification();

        TxtUsername.Focus();
    }

    // ========== NAVEGACIÓN ==========

    private void OpenMainWindow()
    {
        // Permitir cerrar esta ventana ya que el login fue exitoso
        _allowClose = true;
        
        var mainWindow = new MainWindow(_authService, _apiService, _configService);
        mainWindow.Show();
        this.Close();
    }

    /// <summary>
    /// MODO KIOSCO: Prevenir cierre de la ventana excepto cuando el login es exitoso o hay problemas de conexión
    /// </summary>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Permitir cerrar si:
        // 1. El login fue exitoso (_allowClose = true)
        // 2. Hay problemas de conexión con el servidor (_hasConnectionIssue = true)
        if (!_allowClose && !_hasConnectionIssue)
        {
            e.Cancel = true;
            ModernDialog.Show(
                "No puedes cerrar esta ventana.\n\n" +
                "Debes iniciar sesión para continuar.",
                "Acceso Restringido",
                ModernDialog.DialogType.Warning);
        }
        else if (_hasConnectionIssue && !_allowClose)
        {
            // Confirmar cierre cuando hay problemas de conexión
            bool shouldClose = ModernDialog.ShowConfirm(
                "¿Estás seguro de que deseas cerrar la aplicación?\n\n" +
                "No se puede conectar con el servidor. Si cierras ahora, " +
                "la computadora quedará disponible sin restricciones hasta que se resuelva el problema de conexión.",
                "Confirmar Cierre",
                ModernDialog.DialogType.Warning);
            
            if (!shouldClose)
            {
                e.Cancel = true;
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _countdownTimer?.Stop();
    }
}