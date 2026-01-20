namespace HoneyBack.Servicios;

/// <summary>
/// Implementación de email que no envía correos.
/// Se usa cuando la API key de Resend no está configurada.
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
            "[NoOpEmailService] Email NO enviado (API key no configurada). " +
            "To: {ToEmail}, User: {UserName}, Token: {Token}",
            toEmail, userName, token);
        
        // Retorna true para que el flujo continúe (útil en desarrollo)
        return Task.FromResult(true);
    }
}
