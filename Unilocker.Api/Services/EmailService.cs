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
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
                            <div style='background-color: white; padding: 30px; border-radius: 10px;'>
                                <h2 style='color: #333;'>Código de Verificación</h2>
                                <p>Has iniciado sesión en Unilocker. Por favor, ingresa el siguiente código de verificación:</p>
                                <div style='background-color: #007bff; color: white; font-size: 32px; font-weight: bold; text-align: center; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                    {code}
                                </div>
                                <p style='color: #666;'>Este código expira en 10 minutos.</p>
                                <p style='color: #666;'>Si no solicitaste este código, por favor ignora este correo.</p>
                                <hr style='border: 1px solid #eee; margin: 20px 0;'>
                                <p style='color: #999; font-size: 12px;'>Este es un correo automático, por favor no respondas.</p>
                            </div>
                        </div>
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