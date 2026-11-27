using System.Net;
using System.Net.Mail;
using Deber_AutenticacionSegura.Services;

namespace Deber_AutenticacionSegura.Infrastructure;

// Implementación real: usa un proveedor SMTP con STARTTLS.
public class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlMessage)
    {
        var smtp = configuration.GetSection("Smtp");
        var host = smtp["Host"];
        var username = smtp["UserName"];
        var password = smtp["Password"];
        var fromEmail = smtp["FromEmail"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Configura las credenciales SMTP mediante User Secrets o variables de entorno.");

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, smtp["FromName"] ?? "Autenticación segura"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(to);
        using var client = new SmtpClient(host, smtp.GetValue<int?>("Port") ?? 587)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            Timeout = 15000
        };
        await client.SendMailAsync(message);
    }
}
