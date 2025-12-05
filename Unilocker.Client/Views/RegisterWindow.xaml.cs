using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Unilocker.Client.Helpers;
using Unilocker.Client.Models;
using Unilocker.Client.Services;

namespace Unilocker.Client.Views;

public partial class RegisterWindow : Window
{
    private readonly ConfigService _configService;
    private readonly HardwareService _hardwareService;
    private readonly ApiService _apiService;

    private Guid _machineUuid;
    private HardwareInfo _hardwareInfo;
    private List<ClassroomInfo> _classrooms;
    private bool _registrationCompleted = false;

    public RegisterWindow()
    {
        InitializeComponent();

        // Inicializar servicios
        _configService = new ConfigService();
        _hardwareService = new HardwareService();
        _apiService = new ApiService(_configService.GetApiBaseUrl());

        _classrooms = new List<ClassroomInfo>();

        // Cargar datos al iniciar
        Loaded += RegisterWindow_Loaded;
    }

    private async void RegisterWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ShowStatus("Inicializando...", false);
            System.Diagnostics.Debug.WriteLine("=== INICIO DE CARGA ===");

            // 1. Obtener UUID único de la máquina
            _machineUuid = _configService.GetOrCreateMachineId();
            TxtUuid.Text = _machineUuid.ToString().ToUpper();
            System.Diagnostics.Debug.WriteLine($"UUID: {_machineUuid}");

            // 2. Detectar hardware
            _hardwareInfo = _hardwareService.GetHardwareInfo();
            TxtManufacturer.Text = _hardwareInfo.Manufacturer ?? "No detectado";
            TxtModel.Text = _hardwareInfo.Model ?? "No detectado";
            TxtSerial.Text = _hardwareInfo.SerialNumber ?? "No detectado";
            System.Diagnostics.Debug.WriteLine($"Hardware detectado: {_hardwareInfo.Manufacturer} {_hardwareInfo.Model}");

            // 3. Pre-llenar nombre del equipo
            TxtComputerName.Text = _hardwareInfo.ComputerName;

            // 4. Verificar conexión con el API
            System.Diagnostics.Debug.WriteLine($"Intentando conectar a: {_configService.GetApiBaseUrl()}");
            ShowStatus("Conectando con el servidor...", false);

            bool isConnected = await _apiService.TestConnectionAsync();
            System.Diagnostics.Debug.WriteLine($"Conexión exitosa: {isConnected}");

            if (!isConnected)
            {
                ShowStatus("⚠️ No se puede conectar con el servidor. Verifica que la API esté corriendo.", true);
                BtnRegister.IsEnabled = false;
                System.Diagnostics.Debug.WriteLine("ERROR: No se pudo conectar a la API");
                ModernDialog.Show(
                    $"No se puede conectar al servidor en:\n{_configService.GetApiBaseUrl()}\n\n" +
                    "Verifica que:\n" +
                    "1. La API esté corriendo\n" +
                    "2. El puerto sea correcto en appsettings.json",
                    "Error de Conexión",
                    ModernDialog.DialogType.Error);
                return;
            }

            // 5. Cargar aulas disponibles
            ShowStatus("Cargando aulas disponibles...", false);
            System.Diagnostics.Debug.WriteLine("Cargando aulas...");

            _classrooms = await _apiService.GetAvailableClassroomsAsync();
            System.Diagnostics.Debug.WriteLine($"Aulas cargadas: {_classrooms?.Count ?? 0}");

            // DEBUG: Mostrar lo que se recibió
            if (_classrooms != null && _classrooms.Count > 0)
            {
                string debug = $"Aulas recibidas: {_classrooms.Count}\n\n";
                foreach (var aula in _classrooms)
                {
                    debug += $"- {aula.BranchName} / {aula.BlockName} / {aula.Name}\n";
                }
                // DEBUG deshabilitado - ModernDialog.Show(debug, "DEBUG - Aulas", ModernDialog.DialogType.Information);
            }

            if (_classrooms == null || _classrooms.Count == 0)
            {
                ShowStatus("⚠️ No hay aulas disponibles. Contacta al administrador.", true);
                BtnRegister.IsEnabled = false;
                ModernDialog.Show(
                    "No se encontraron aulas disponibles en el sistema.\n\n" +
                    "Contacta al administrador para crear aulas.",
                    "Sin Aulas",
                    ModernDialog.DialogType.Warning);
                return;
            }

            // Crear lista con formato: "Campus - Bloque - Aula"
            var classroomItems = _classrooms.Select(c => new
            {
                c.Id,
                DisplayName = $"{c.BranchName} - {c.BlockName} - {c.Name}"
            }).ToList();

            CmbClassroom.ItemsSource = classroomItems;
            System.Diagnostics.Debug.WriteLine($"ComboBox cargado con {classroomItems.Count} items");

            ShowStatus("✓ Listo para registrar", false);
            System.Diagnostics.Debug.WriteLine("=== CARGA COMPLETADA ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR CRÍTICO: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            ShowStatus($"❌ Error al inicializar: {ex.Message}", true);
            BtnRegister.IsEnabled = false;

            MessageBox.Show(
                $"Error al inicializar la aplicación:\n\n{ex.Message}\n\n" +
                $"Detalles técnicos:\n{ex.GetType().Name}",
                "Error Crítico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(TxtComputerName.Text))
        {
            ShowStatus("⚠️ Debe ingresar el nombre del equipo", true);
            TxtComputerName.Focus();
            return;
        }

        if (CmbClassroom.SelectedValue == null)
        {
            ShowStatus("⚠️ Debe seleccionar un aula", true);
            CmbClassroom.Focus();
            return;
        }

        try
        {
            // Deshabilitar controles
            BtnRegister.IsEnabled = false;
            TxtComputerName.IsEnabled = false;
            CmbClassroom.IsEnabled = false;
            ShowProgress("Registrando equipo...");

            // Crear request
            var request = new RegisterComputerRequest
            {
                Name = TxtComputerName.Text.Trim(),
                Uuid = _machineUuid,
                SerialNumber = _hardwareInfo.SerialNumber,
                Model = _hardwareInfo.Model,
                ClassroomId = (int)CmbClassroom.SelectedValue
            };

            // Registrar en el API
            var response = await _apiService.RegisterComputerAsync(request);

            // Marcar como registrado
            _configService.MarkAsRegistered(response.Id);

            // Guardar el Computer ID para usar en sesiones
            _configService.SaveComputerId(response.Id);

            HideProgress();

            // Mostrar mensaje de éxito en la misma ventana
            string message = response.IsNewRegistration
                ? $"✓ Equipo registrado exitosamente!\n\n" +
                  $"ID: {response.Id}\n" +
                  $"Nombre: {response.Name}\n" +
                  $"Aula: {response.ClassroomInfo?.Name}\n\n" +
                  $"Esta computadora ya está lista para usar el sistema.\n\n" +
                  $"⏱️ Esta ventana se cerrará automáticamente en 4 segundos..."
                : $"ℹ️ Este equipo ya estaba registrado.\n\n" +
                  $"ID: {response.Id}\n" +
                  $"Nombre: {response.Name}\n" +
                  $"Aula: {response.ClassroomInfo?.Name}\n\n" +
                  $"No es necesario volver a registrarlo.\n\n" +
                  $"⏱️ Esta ventana se cerrará automáticamente en 4 segundos...";

            _registrationCompleted = true;

            // Deshabilitar controles
            BtnRegister.IsEnabled = false;
            TxtComputerName.IsEnabled = false;
            CmbClassroom.IsEnabled = false;

            // Mostrar mensaje de éxito en el StatusBorder
            ShowStatus(message, false);

            // Cerrar automáticamente después de 4 segundos
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            HideProgress();
            ShowStatus($"❌ Error al registrar: {ex.Message}", true);

            // Re-habilitar controles
            BtnRegister.IsEnabled = true;
            TxtComputerName.IsEnabled = true;
            CmbClassroom.IsEnabled = true;
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
        TxtStatus.Visibility = Visibility.Collapsed;
    }

    private void ShowProgress(string message)
    {
        ShowStatus(message, false);
        ProgressBar.Visibility = Visibility.Visible;
    }

    private void HideProgress()
    {
        ProgressBar.Visibility = Visibility.Collapsed;
    }
}