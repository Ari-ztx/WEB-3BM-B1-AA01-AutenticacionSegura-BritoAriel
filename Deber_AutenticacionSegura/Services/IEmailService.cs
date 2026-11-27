namespace Deber_AutenticacionSegura.Services;
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlMessage);
}
