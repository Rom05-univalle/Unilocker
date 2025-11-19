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

        // Verificar si la computadora ya está registrada
        if (configService.IsComputerRegistered())
        {
            // Ya está registrado - mostrar mensaje y cerrar
            MessageBox.Show(
                "Este equipo ya está registrado en el sistema Unilocker.\n\n" +
                "No es necesario volver a registrarlo.",
                "Equipo Registrado",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // Cerrar la aplicación
            Shutdown();
        }
        else
        {
            // No está registrado - mostrar ventana de registro
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
        }
    }
}