using System;
using System.Windows;
using System.Windows.Input;
using Unilocker.Client.Services;

namespace Unilocker.Client.Views;

public partial class LoginWindow : Window
{
    private readonly ConfigService _configService;
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly HardwareService _hardwareService;

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
        Loaded += (s, e) => TxtUsername.Focus();
    }

    private void ShowComputerInfo()
    {
        try
        {
            var hardwareInfo = _hardwareService.GetHardwareInfo();
            Guid machineId = _configService.GetOrCreateMachineId();

            TxtComputerInfo.Text = $"Equipo: {hardwareInfo.ComputerName} | Marca: {hardwareInfo.Manufacturer ?? "N/A"} | ID: {machineId.ToString().Substring(0, 8)}...";
        }
        catch
        {
            TxtComputerInfo.Text = "No se pudo obtener información del equipo";
        }
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        await PerformLoginAsync();
    }

    private async void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformLoginAsync();
        }
    }

    private async System.Threading.Tasks.Task PerformLoginAsync()
    {
        // Validar campos
        string username = TxtUsername.Text.Trim();
        string password = TxtPassword.Password;

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

            // Login exitoso
            ShowSuccess($"Bienvenido, {loginResponse.Username}!");

            // Esperar un momento para que el usuario vea el mensaje
            await System.Threading.Tasks.Task.Delay(800);

            // Abrir ventana principal
            var mainWindow = new MainWindow(_authService, _apiService, _configService);
            mainWindow.Show();

            // Cerrar ventana de login
            this.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Error al iniciar sesión: {ex.Message}");
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
        TxtError.Visibility = Visibility.Visible;
    }

    private void HideError()
    {
        TxtError.Text = "";
        TxtError.Visibility = Visibility.Collapsed;
    }

    private void ShowSuccess(string message)
    {
        TxtError.Text = message;
        TxtError.Foreground = System.Windows.Media.Brushes.Green;
        TxtError.Visibility = Visibility.Visible;
    }
}