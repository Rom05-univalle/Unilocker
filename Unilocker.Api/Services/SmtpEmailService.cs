using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Unilocker.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly string smtpHost = "smtp.gmail.com";
    private readonly int smtpPort = 587;
    private readonly string smtpUser = "unilockerunivalle@gmail.com";      // <-- aquí tu Gmail
    private readonly string smtpPass = "nkox itdv xpjz qtzj";          // <-- aquí la App Password de 16 caracteres

    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPass)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(smtpUser, "UniLocker"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }
}
