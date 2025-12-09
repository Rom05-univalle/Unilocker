using System.Windows;
using System.IO;
using Unilocker.Client.Helpers;
using Unilocker.Client.Services;
using Unilocker.Client.Views;

namespace Unilocker.Client;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configService = new ConfigService();

        // PASO 1: Verificar si la computadora está registrada
        if (!configService.IsComputerRegistered())
        {
            // No está registrado - mostrar ventana de registro
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();

            // Después del registro, cerrar la aplicación
            // El usuario debe ejecutar nuevamente para hacer login
            ModernDialog.Show(
                "Equipo registrado exitosamente.\n\n" +
                "Por favor, ejecuta la aplicación nuevamente para iniciar sesión.",
                "Registro Completado",
                ModernDialog.DialogType.Success);

            Shutdown();
            return;
        }

        // PASO 2: Habilitar inicio automático si no está habilitado
        if (!configService.IsStartupEnabled())
        {
            bool enabled = configService.SetStartupEnabled(true);
            if (enabled)
            {
                System.Diagnostics.Debug.WriteLine("✓ Inicio automático habilitado");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠ No se pudo habilitar el inicio automático");
            }
        }

        // PASO 3: El equipo ya está registrado
        // SIEMPRE mostrar LoginWindow (no restaurar sesión automáticamente)
        // Esto permite que múltiples usuarios usen el mismo equipo
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}