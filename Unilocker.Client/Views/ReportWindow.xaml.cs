using System;
using System.Windows;
using Unilocker.Client.Helpers;
using Unilocker.Client.Services;

namespace Unilocker.Client.Views;

public partial class ReportWindow : Window
{
    private readonly ReportsService _reportsService;
    private readonly int _sessionId;
    private bool _reportSubmitted = false;

    public bool ReportSubmitted => _reportSubmitted;

    public ReportWindow(ReportsService reportsService, int sessionId)
    {
        InitializeComponent();

        _reportsService = reportsService;
        _sessionId = sessionId;

        // Cargar tipos de problema al iniciar
        Loaded += ReportWindow_Loaded;

        // Evitar que se pueda cerrar con Alt+F4 o la X
        Closing += (s, e) =>
        {
            // Solo permitir cerrar si ya se eligió una opción
            if (!_reportSubmitted && !_skipPressed)
            {
                e.Cancel = true;
                ModernDialog.Show(
                    "Por favor, elige una opción:\n\n" +
                    "- Enviar Reporte si tuviste problemas\n" +
                    "- Cerrar Sin Reportar si todo funcionó bien",
                    "Acción Requerida",
                    ModernDialog.DialogType.Information);
            }
        };
    }

    private bool _skipPressed = false;

    private async void ReportWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ShowStatus("Cargando tipos de problema...", false);

            var problemTypes = await _reportsService.GetProblemTypesAsync();

            if (problemTypes == null || problemTypes.Count == 0)
            {
                ShowStatus("⚠️ No se pudieron cargar los tipos de problema. Puedes cerrar sin reportar.", true);
                BtnSubmit.IsEnabled = false;
                return;
            }

            CmbProblemType.ItemsSource = problemTypes;
            HideStatus();

            // Pre-seleccionar el primer elemento
            if (problemTypes.Count > 0)
            {
                CmbProblemType.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al cargar tipos de problema: {ex.Message}", true);
            BtnSubmit.IsEnabled = false;
        }
    }

    private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
    {
        // Validar que se haya seleccionado un tipo de problema
        if (CmbProblemType.SelectedValue == null)
        {
            ShowStatus("Por favor selecciona un tipo de problema", true);
            CmbProblemType.Focus();
            return;
        }

        try
        {
            // Deshabilitar controles
            SetLoadingState(true);
            ShowStatus("Enviando reporte...", false);

            int problemTypeId = (int)CmbProblemType.SelectedValue;
            string description = TxtDescription.Text.Trim();

            // Si no hay descripción, poner un mensaje por defecto
            if (string.IsNullOrEmpty(description))
            {
                description = "Sin descripción adicional";
            }

            // Enviar reporte
            bool success = await _reportsService.CreateReportAsync(_sessionId, problemTypeId, description);

            if (success)
            {
                _reportSubmitted = true;
                ModernDialog.Show(
                    "✓ Reporte enviado exitosamente.\n\nGracias por reportar el problema. El equipo técnico lo revisará pronto.",
                    "Reporte Enviado",
                    ModernDialog.DialogType.Success);
                this.Close();
            }
            else
            {
                ShowStatus("Error al enviar el reporte. Intenta nuevamente o cierra sin reportar.", true);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al enviar reporte: {ex.Message}", true);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void BtnSkip_Click(object sender, RoutedEventArgs e)
    {
        var result = ModernDialog.ShowConfirm(
            "¿Estás seguro de que deseas cerrar sin reportar?\n\n" +
            "Si tuviste algún problema técnico, es importante reportarlo para mejorar el servicio.",
            "Confirmar",
            ModernDialog.DialogType.Question);

        if (result)
        {
            _skipPressed = true;
            _reportSubmitted = false;
            this.Close();
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        BtnSubmit.IsEnabled = !isLoading;
        BtnSkip.IsEnabled = !isLoading;
        CmbProblemType.IsEnabled = !isLoading;
        TxtDescription.IsEnabled = !isLoading;
        ProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowStatus(string message, bool isError)
    {
        TxtStatus.Text = message;
        TxtStatus.Foreground = isError
            ? System.Windows.Media.Brushes.Red
            : System.Windows.Media.Brushes.Green;
        TxtStatus.Visibility = Visibility.Visible;
    }

    private void HideStatus()
    {
        TxtStatus.Visibility = Visibility.Collapsed;
    }
}