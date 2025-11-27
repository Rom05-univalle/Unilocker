using System.Windows;
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
            MessageBox.Show(
                "Equipo registrado exitosamente.\n\n" +
                "Por favor, ejecuta la aplicación nuevamente para iniciar sesión.",
                "Registro Completado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Shutdown();
            return;
        }

        // PASO 2: El equipo ya está registrado
        // SIEMPRE mostrar LoginWindow (no restaurar sesión automáticamente)
        // Esto permite que múltiples usuarios usen el mismo equipo
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}