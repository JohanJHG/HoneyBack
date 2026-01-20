using Resend;

namespace HoneyBack.Servicios;

/// <summary>
/// Implementación del servicio de email usando Resend.
/// La API key se configura via DI desde User Secrets (desarrollo) o variables de entorno (producción).
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public ResendEmailService(
        IResend resend,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _logger = logger;
        
        // Configuración del remitente (usar dominio verificado en Resend)
        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
        _fromName = configuration["Resend:FromName"] ?? "HoneyBalance";
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token)
    {
        try
        {
            var htmlContent = GeneratePasswordResetEmailTemplate(userName, token);
            var plainTextContent = $"Hola {userName}, tu codigo de recuperacion es: {token}. Este codigo expira en 15 minutos.";

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = toEmail,
                Subject = "Recupera tu contrasena - HoneyBalance",
                HtmlBody = htmlContent,
                TextBody = plainTextContent
            };

            var response = await _resend.EmailSendAsync(message);

            if (response != null)
            {
                _logger.LogInformation(
                    "Email de recuperacion enviado exitosamente a {ToEmail}",
                    toEmail);
                return true;
            }

            _logger.LogWarning("Respuesta vacia de Resend para email a {ToEmail}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de recuperacion a {ToEmail}", toEmail);
            return false;
        }
    }

    private static string GeneratePasswordResetEmailTemplate(string userName, string token)
    {
        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #1a1a1a;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; max-width: 100%; border-collapse: collapse; background-color: #0F0F0F; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.3);"">
                    <tr>
                        <td style=""padding: 40px 40px 20px; text-align: center; background: linear-gradient(135deg, #FFD8A9 0%, #E8A87C 100%);"">
                            <h1 style=""margin: 0; font-size: 28px; font-weight: 700; color: #0F0F0F;"">
                                HoneyBalance
                            </h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""margin: 0 0 20px; font-size: 24px; font-weight: 600; color: #F9F9F9;"">
                                Hola {System.Net.WebUtility.HtmlEncode(userName)},
                            </h2>
                            <p style=""margin: 0 0 30px; font-size: 16px; line-height: 1.6; color: #CCCCCC;"">
                                Recibimos una solicitud para restablecer la contrasena de tu cuenta.
                                Usa el siguiente codigo para completar el proceso:
                            </p>
                            <div style=""background: linear-gradient(135deg, #1a1a1a 0%, #252525 100%); border: 2px solid #FFD8A9; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0;"">
                                <p style=""margin: 0 0 10px; font-size: 14px; color: #CCCCCC; text-transform: uppercase; letter-spacing: 2px;"">
                                    Tu codigo de verificacion
                                </p>
                                <h1 style=""margin: 0; font-size: 48px; font-weight: 700; color: #FFD8A9; letter-spacing: 12px; font-family: monospace;"">
                                    {token}
                                </h1>
                                <p style=""margin: 15px 0 0; font-size: 13px; color: #999999;"">
                                    Este codigo expira en <strong style=""color: #FFD8A9;"">15 minutos</strong>
                                </p>
                            </div>
                            <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #999999;"">
                                Si no solicitaste restablecer tu contrasena, puedes ignorar este correo de forma segura.
                                Tu contrasena actual permanecera sin cambios.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #0a0a0a; border-top: 1px solid #2a2a2a;"">
                            <p style=""margin: 0 0 10px; font-size: 12px; color: #666666; text-align: center;"">
                                Este es un correo automatico, por favor no respondas a este mensaje.
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #666666; text-align: center;"">
                                HoneyBalance. Todos los derechos reservados.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
