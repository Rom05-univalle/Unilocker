using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using Unilocker.Client.Services;

namespace Unilocker.Client.Views;

public partial class SetupWindow : Window
{
    private bool _connectionTested = false;
    private readonly string _appSettingsPath;

    public SetupWindow()
    {
        InitializeComponent();
        
        _appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        
        // Cargar URL actual si existe
        LoadCurrentApiUrl();
    }

    private void LoadCurrentApiUrl()
    {
        try
        {
            if (File.Exists(_appSettingsPath))
            {
                string json = File.ReadAllText(_appSettingsPath);
                var settings = JObject.Parse(json);
                string? currentUrl = settings["ApiSettings"]?["BaseUrl"]?.ToString();
                
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    TxtApiUrl.Text = currentUrl;
                }
            }
        }
        catch
        {
            // Usar valor por defecto si falla
        }
    }

    private async void BtnTestConnection_Click(object sender, RoutedEventArgs e)
    {
        string apiUrl = TxtApiUrl.Text.Trim();

        // Validar formato
        if (string.IsNullOrEmpty(apiUrl))
        {
            ShowStatus("⚠️ Por favor ingresa una URL válida", true);
            return;
        }

        if (!apiUrl.StartsWith("http://") && !apiUrl.StartsWith("https://"))
        {
            ShowStatus("⚠️ La URL debe comenzar con http:// o https://", true);
            return;
        }

        // Remover trailing slash si existe
        apiUrl = apiUrl.TrimEnd('/');
        TxtApiUrl.Text = apiUrl;

        // Probar conexión
        BtnTestConnection.IsEnabled = false;
        BtnSave.IsEnabled = false;
        ProgressBar.Visibility = Visibility.Visible;
        HideStatus();

        try
        {
            var apiService = new ApiService(apiUrl);
            bool isConnected = await apiService.TestConnectionAsync();

            ProgressBar.Visibility = Visibility.Collapsed;

            if (isConnected)
            {
                ShowStatus("✅ Conexión exitosa con el servidor", false);
                _connectionTested = true;
                BtnSave.IsEnabled = true;
            }
            else
            {
                ShowStatus("❌ No se pudo conectar al servidor. Verifica la URL y que la API esté corriendo.", true);
                _connectionTested = false;
                BtnSave.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ShowStatus($"❌ Error al conectar: {ex.Message}", true);
            _connectionTested = false;
            BtnSave.IsEnabled = false;
        }
        finally
        {
            BtnTestConnection.IsEnabled = true;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!_connectionTested)
        {
            ShowStatus("⚠️ Debes probar la conexión primero", true);
            return;
        }

        try
        {
            string apiUrl = TxtApiUrl.Text.Trim().TrimEnd('/');

            // Cargar appsettings.json actual o crear uno nuevo
            JObject settings;
            
            if (File.Exists(_appSettingsPath))
            {
                string json = File.ReadAllText(_appSettingsPath);
                settings = JObject.Parse(json);
            }
            else
            {
                settings = new JObject();
            }

            // Actualizar o crear sección ApiSettings
            if (settings["ApiSettings"] == null)
            {
                settings["ApiSettings"] = new JObject();
            }

            settings["ApiSettings"]!["BaseUrl"] = apiUrl;

            // Asegurar que existe AppSettings
            if (settings["AppSettings"] == null)
            {
                settings["AppSettings"] = new JObject
                {
                    ["DataDirectory"] = @"C:\ProgramData\Unilocker",
                    ["MachineIdFile"] = "machine.id",
                    ["RegisteredFlagFile"] = "registered.flag"
                };
            }

            // Guardar archivo
            string updatedJson = settings.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_appSettingsPath, updatedJson);

            MessageBox.Show(
                $"✅ Configuración guardada exitosamente.\n\n" +
                $"URL de la API: {apiUrl}\n\n" +
                "Ahora puedes continuar con el registro de este equipo.",
                "Configuración Guardada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowStatus($"❌ Error al guardar configuración: {ex.Message}", true);
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        TxtStatus.Text = message;
        
        if (isError)
        {
            StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 235, 238)); // Rojo claro
            StatusBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(244, 67, 54)); // Rojo
            TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(198, 40, 40)); // Rojo oscuro
        }
        else
        {
            StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(232, 245, 233)); // Verde claro
            StatusBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80)); // Verde
            TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(46, 125, 50)); // Verde oscuro
        }
        
        StatusBorder.Visibility = Visibility.Visible;
    }

    private void HideStatus()
    {
        StatusBorder.Visibility = Visibility.Collapsed;
    }
}
