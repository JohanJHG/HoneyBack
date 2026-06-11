namespace HoneyBack.Servicios;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token);

    Task<bool> SendContactNotificationAsync(string fromName, string fromEmail, string mensaje);

    Task<bool> SendAdminReplyAsync(string toEmail, string toName, string asunto, string cuerpo);
}
