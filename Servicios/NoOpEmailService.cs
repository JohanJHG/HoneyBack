namespace HoneyBack.Servicios;

/// <summary>
/// Implementaci�n de email que no env�a correos.
/// Se usa cuando la API key de Resend no est� configurada.
/// Solo registra en logs para desarrollo/testing.
/// </summary>
public class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token)
    {
        _logger.LogWarning(
            "[NoOpEmailService] Email de recuperación NO enviado (API key no configurada). To={ToEmail} User={UserName} Token={Token}",
            toEmail, userName, token);
        return Task.FromResult(true);
    }

    public Task<bool> SendContactNotificationAsync(string fromName, string fromEmail, string mensaje)
    {
        _logger.LogWarning(
            "[NoOpEmailService] Notificación de contacto NO enviada (API key no configurada). From={FromEmail} Nombre={FromName}",
            fromEmail, fromName);
        return Task.FromResult(true);
    }

    public Task<bool> SendAdminReplyAsync(string toEmail, string toName, string asunto, string cuerpo)
    {
        _logger.LogWarning(
            "[NoOpEmailService] Respuesta admin NO enviada (API key no configurada). To={ToEmail} Asunto={Asunto}",
            toEmail, asunto);
        return Task.FromResult(true);
    }
}
