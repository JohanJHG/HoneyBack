namespace HoneyBack.Servicios;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token);

    /// <summary>
    /// Notifica a soporte cuando llega un nuevo mensaje de contacto o recomendación.
    /// </summary>
    Task<bool> SendContactNotificationAsync(string fromName, string fromEmail, string mensaje);
}
