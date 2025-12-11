using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Unilocker.Api.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Envía un código de verificación por email
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="code">Código de verificación de 6 dígitos</param>
    /// <returns>True si se envió correctamente, False si hubo error</returns>
    public async Task<bool> SendVerificationCodeAsync(string toEmail, string code)
    {
        try
        {
            _logger.LogInformation("📧 Enviando código de verificación a: {Email}", toEmail);

            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];
            var password = _configuration["Email:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Código de Verificación - Unilocker";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    </head>
                    <body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f4f4;'>
                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px 0;'>
                            <tr>
                                <td align='center'>
                                    <!-- Contenedor principal -->
                                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); overflow: hidden;'>
                                        <!-- Header con colores Univalle -->
                                        <tr>
                                            <td style='background: linear-gradient(135deg, #800020 0%, #a0002a 100%); padding: 40px 30px; text-align: center;'>
                                                <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);'>
                                                    🔐 UNILOCKER
                                                </h1>
                                                <p style='margin: 10px 0 0 0; color: #f0f0f0; font-size: 14px; letter-spacing: 1px;'>
                                                    Universidad Privada del Valle
                                                </p>
                                            </td>
                                        </tr>
                                        
                                        <!-- Contenido -->
                                        <tr>
                                            <td style='padding: 40px 30px;'>
                                                <h2 style='margin: 0 0 20px 0; color: #800020; font-size: 24px; font-weight: bold;'>
                                                    Código de Verificación de Seguridad
                                                </h2>
                                                
                                                <p style='margin: 0 0 20px 0; color: #333333; font-size: 16px; line-height: 1.6;'>
                                                    Hola,
                                                </p>
                                                
                                                <p style='margin: 0 0 30px 0; color: #555555; font-size: 15px; line-height: 1.6;'>
                                                    Has iniciado sesión en el sistema <strong>Unilocker</strong> de control de laboratorios. 
                                                    Por favor, utiliza el siguiente código de verificación para completar tu autenticación:
                                                </p>
                                                
                                                <!-- Código de verificación -->
                                                <table width='100%' cellpadding='0' cellspacing='0' style='margin: 30px 0;'>
                                                    <tr>
                                                        <td align='center'>
                                                            <div style='background: linear-gradient(135deg, #800020 0%, #a0002a 100%); 
                                                                        color: #ffffff; 
                                                                        font-size: 42px; 
                                                                        font-weight: bold; 
                                                                        letter-spacing: 8px; 
                                                                        padding: 25px 40px; 
                                                                        border-radius: 12px; 
                                                                        box-shadow: 0 4px 15px rgba(128, 0, 32, 0.3);
                                                                        display: inline-block;
                                                                        text-shadow: 2px 2px 4px rgba(0,0,0,0.2);'>
                                                                {code}
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                                
                                                <!-- Advertencias -->
                                                <div style='background-color: #fff3cd; 
                                                            border-left: 4px solid #ffc107; 
                                                            padding: 15px 20px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0; color: #856404; font-size: 14px; line-height: 1.5;'>
                                                        ⏱️ <strong>Importante:</strong> Este código expira en <strong>10 minutos</strong>.
                                                    </p>
                                                </div>
                                                
                                                <div style='background-color: #f8d7da; 
                                                            border-left: 4px solid #dc3545; 
                                                            padding: 15px 20px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0; color: #721c24; font-size: 14px; line-height: 1.5;'>
                                                        🔒 <strong>Seguridad:</strong> Si no solicitaste este código, ignora este correo y 
                                                        contacta inmediatamente al administrador del sistema.
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                        
                                        <!-- Footer -->
                                        <tr>
                                            <td style='background-color: #f8f9fa; padding: 25px 30px; border-top: 3px solid #800020;'>
                                                <p style='margin: 0 0 10px 0; color: #666666; font-size: 13px; line-height: 1.5;'>
                                                    <strong>Unilocker</strong> - Sistema de Control de Acceso a Laboratorios
                                                </p>
                                                <p style='margin: 0; color: #999999; font-size: 12px; line-height: 1.5;'>
                                                    Universidad Privada del Valle (Univalle)<br>
                                                    Este es un correo automático, por favor no respondas a este mensaje.
                                                </p>
                                            </td>
                                        </tr>
                                        
                                        <!-- Barra inferior Univalle -->
                                        <tr>
                                            <td style='background-color: #800020; padding: 5px;'></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>
                "
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email enviado exitosamente a: {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar email a: {Email}", toEmail);
            return false;
        }
    }
    /// <summary>
    /// Envía la contraseña generada automáticamente al nuevo usuario
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="fullName">Nombre completo del usuario</param>
    /// <param name="password">Contraseña generada</param>
    /// <returns>True si se envió correctamente, False si hubo error</returns>
    public async Task<bool> SendPasswordAsync(string toEmail, string username, string fullName, string password)
    {
        try
        {
            _logger.LogInformation("📧 Enviando contraseña generada a: {Email}", toEmail);

            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];
            var emailPassword = _configuration["Email:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Bienvenido a Unilocker - Tu contraseña de acceso";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    </head>
                    <body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f4f4;'>
                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px 0;'>
                            <tr>
                                <td align='center'>
                                    <!-- Contenedor principal -->
                                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); overflow: hidden;'>
                                        <!-- Header con colores Univalle -->
                                        <tr>
                                            <td style='background: linear-gradient(135deg, #800020 0%, #a0002a 100%); padding: 40px 30px; text-align: center;'>
                                                <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);'>
                                                    🔐 UNILOCKER
                                                </h1>
                                                <p style='margin: 10px 0 0 0; color: #f0f0f0; font-size: 14px; letter-spacing: 1px;'>
                                                    Universidad Privada del Valle
                                                </p>
                                            </td>
                                        </tr>
                                        
                                        <!-- Contenido -->
                                        <tr>
                                            <td style='padding: 40px 30px;'>
                                                <h2 style='margin: 0 0 20px 0; color: #800020; font-size: 24px; font-weight: bold;'>
                                                    ¡Bienvenido a Unilocker!
                                                </h2>
                                                
                                                <p style='margin: 0 0 20px 0; color: #333333; font-size: 16px; line-height: 1.6;'>
                                                    Hola <strong>{fullName}</strong>,
                                                </p>
                                                
                                                <p style='margin: 0 0 30px 0; color: #555555; font-size: 15px; line-height: 1.6;'>
                                                    Tu cuenta en el sistema <strong>Unilocker</strong> de control de laboratorios ha sido creada exitosamente. 
                                                    A continuación encontrarás tus credenciales de acceso:
                                                </p>
                                                
                                                <!-- Credenciales -->
                                                <div style='background-color: #f8f9fa; 
                                                            border-left: 4px solid #800020; 
                                                            padding: 20px 25px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0 0 15px 0; color: #333333; font-size: 15px;'>
                                                        <strong>👤 Usuario:</strong> <span style='font-family: Courier, monospace; background-color: #e9ecef; padding: 5px 10px; border-radius: 3px;'>{username}</span>
                                                    </p>
                                                    <p style='margin: 0; color: #333333; font-size: 15px;'>
                                                        <strong>🔑 Contraseña:</strong> <span style='font-family: Courier, monospace; background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; font-size: 18px; font-weight: bold; color: #800020;'>{password}</span>
                                                    </p>
                                                </div>
                                                
                                                <!-- Instrucciones -->
                                                <div style='background-color: #d1ecf1; 
                                                            border-left: 4px solid #0c5460; 
                                                            padding: 15px 20px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0 0 10px 0; color: #0c5460; font-size: 14px; font-weight: bold;'>
                                                        📋 Instrucciones de primer acceso:
                                                    </p>
                                                    <ol style='margin: 5px 0 0 20px; padding: 0; color: #0c5460; font-size: 14px; line-height: 1.6;'>
                                                        <li>Ingresa al sistema con las credenciales proporcionadas</li>
                                                        <li>Se te solicitará un código de verificación en tu correo</li>
                                                        <li>Una vez dentro, podrás cambiar tu contraseña si lo deseas</li>
                                                    </ol>
                                                </div>
                                                
                                                <!-- Advertencias de seguridad -->
                                                <div style='background-color: #fff3cd; 
                                                            border-left: 4px solid #ffc107; 
                                                            padding: 15px 20px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0; color: #856404; font-size: 14px; line-height: 1.5;'>
                                                        ⚠️ <strong>Importante:</strong> Guarda esta contraseña en un lugar seguro. Podrás cambiarla después de tu primer inicio de sesión.
                                                    </p>
                                                </div>
                                                
                                                <div style='background-color: #f8d7da; 
                                                            border-left: 4px solid #dc3545; 
                                                            padding: 15px 20px; 
                                                            margin: 25px 0; 
                                                            border-radius: 5px;'>
                                                    <p style='margin: 0; color: #721c24; font-size: 14px; line-height: 1.5;'>
                                                        🔒 <strong>Seguridad:</strong> Nunca compartas tu contraseña con nadie. Si no solicitaste esta cuenta, 
                                                        contacta inmediatamente al administrador del sistema.
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                        
                                        <!-- Footer -->
                                        <tr>
                                            <td style='background-color: #f8f9fa; padding: 25px 30px; border-top: 3px solid #800020;'>
                                                <p style='margin: 0 0 10px 0; color: #666666; font-size: 13px; line-height: 1.5;'>
                                                    <strong>Unilocker</strong> - Sistema de Control de Acceso a Laboratorios
                                                </p>
                                                <p style='margin: 0; color: #999999; font-size: 12px; line-height: 1.5;'>
                                                    Universidad Privada del Valle (Univalle)<br>
                                                    Este es un correo automático, por favor no respondas a este mensaje.
                                                </p>
                                            </td>
                                        </tr>
                                        
                                        <!-- Barra inferior Univalle -->
                                        <tr>
                                            <td style='background-color: #800020; padding: 5px;'></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>
                "
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, emailPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email con contraseña enviado exitosamente a: {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar email con contraseña a: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Enmascara un email para mostrarlo parcialmente (ej: j***@gmail.com)
    /// </summary>
    public string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***@***.com";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length > 2
            ? $"{localPart[0]}***{localPart[^1]}"
            : $"{localPart[0]}***";

        return $"{maskedLocal}@{domain}";
    }
}