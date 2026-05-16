using Resend;
using System.Net;

namespace HoneyBack.Servicios;

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
        _fromEmail = configuration["Resend:FromEmail"] ?? "soporte@honeybalance.online";
        _fromName = configuration["Resend:FromName"] ?? "HoneyBalance";
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token)
    {
        try
        {
            var htmlContent = GeneratePasswordResetEmailTemplate(userName, token);
            var plainTextContent = $"Hola {userName}, tu código de recuperación es: {token}. Expira en 15 minutos. Si no lo solicitaste, ignora este mensaje.";

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = toEmail,
                Subject = "Código de recuperación — HoneyBalance",
                HtmlBody = htmlContent,
                TextBody = plainTextContent
            };

            var response = await _resend.EmailSendAsync(message);

            if (response != null)
            {
                _logger.LogInformation("Email de recuperación enviado a {ToEmail}", toEmail);
                return true;
            }

            _logger.LogWarning("Respuesta vacía de Resend para {ToEmail}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de recuperación a {ToEmail}", toEmail);
            return false;
        }
    }

    private static string GeneratePasswordResetEmailTemplate(string userName, string token)
    {
        var safeName = WebUtility.HtmlEncode(userName);

        return $@"<!DOCTYPE html>
<html lang=""es"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
  <link href=""https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap"" rel=""stylesheet"">
  <title>Recupera tu contraseña — HoneyBalance</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');
    body, table, td, p, a {{ margin: 0; padding: 0; }}
    body {{ background-color: #111111; }}
  </style>
</head>
<body style=""margin:0;padding:0;background-color:#111111;-webkit-font-smoothing:antialiased;"">

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""
         style=""background-color:#111111;"">
    <tr>
      <td align=""center"" style=""padding:48px 16px;"">

        <!-- Card -->
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0""
               style=""max-width:560px;width:100%;background-color:#171717;border-radius:20px;
                       overflow:hidden;border:1px solid #2A2A2A;"">

          <!-- HEADER -->
          <tr>
            <td align=""center""
                style=""background:linear-gradient(135deg,#FFD8A9 0%,#F0B060 50%,#E8A840 100%);
                        padding:36px 40px 30px;"">
              <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                         font-size:30px;font-weight:800;color:#0F0F0F;letter-spacing:-1px;
                         margin:0 0 6px;line-height:1;"">
                HoneyBalance
              </p>
              <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                         font-size:11px;font-weight:600;color:rgba(0,0,0,0.45);
                         letter-spacing:2px;text-transform:uppercase;margin:0;"">
                Gestión financiera personal
              </p>
            </td>
          </tr>

          <!-- DIVIDER -->
          <tr>
            <td style=""height:1px;background:linear-gradient(90deg,transparent,#FFD8A9,transparent);""></td>
          </tr>

          <!-- BODY -->
          <tr>
            <td style=""padding:40px 44px 36px;"">

              <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                         font-size:22px;font-weight:700;color:#F9F9F9;margin:0 0 12px;letter-spacing:-0.3px;"">
                Hola, {safeName}
              </p>

              <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                         font-size:15px;font-weight:400;color:#AAAAAA;line-height:1.7;margin:0 0 32px;"">
                Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                Usa el siguiente código para continuar.
                <strong style=""color:#F9F9F9;"">Expira en 15 minutos.</strong>
              </p>

              <!-- OTP label -->
              <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                         font-size:11px;font-weight:600;color:#FFD8A9;letter-spacing:2px;
                         text-transform:uppercase;margin:0 0 12px;"">
                Tu código de verificación
              </p>

              <!-- OTP block (copiable) -->
              <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom:32px;"">
                <tr>
                  <td style=""background-color:#1C1C1C;border:2px solid #FFD8A9;border-radius:12px;
                              padding:18px 36px;text-align:center;"">
                    <span style=""font-family:'Courier New',Courier,monospace;
                                  font-size:40px;font-weight:700;color:#FFD8A9;
                                  letter-spacing:10px;user-select:all;"">
                      {token}
                    </span>
                  </td>
                </tr>
              </table>

              <!-- Security note -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                  <td style=""background-color:#1C1C1C;border-left:3px solid #FFD8A9;
                              border-radius:0 8px 8px 0;padding:14px 18px;"">
                    <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                               font-size:13px;font-weight:400;color:#888888;line-height:1.6;margin:0;"">
                      Si <strong style=""color:#AAAAAA;"">no solicitaste</strong> este código,
                      puedes ignorar este correo con seguridad. Tu contraseña actual no cambiará.
                    </p>
                  </td>
                </tr>
              </table>

            </td>
          </tr>

          <!-- FOOTER -->
          <tr>
            <td style=""height:1px;background-color:#222222;""></td>
          </tr>
          <tr>
            <td style=""padding:22px 44px 26px;background-color:#131313;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                  <td>
                    <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                               font-size:12px;font-weight:600;color:#FFD8A9;margin:0 0 4px;"">
                      HoneyBalance
                    </p>
                    <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                               font-size:11px;font-weight:400;color:#555555;margin:0;line-height:1.6;"">
                      Mensaje automático, por favor no respondas a este correo.<br>
                      <a href=""mailto:soporte@honeybalance.online""
                         style=""color:#FFD8A9;text-decoration:none;"">soporte@honeybalance.online</a>
                    </p>
                  </td>
                  <td align=""right"" valign=""middle"">
                    <p style=""font-family:'Plus Jakarta Sans','Segoe UI',-apple-system,BlinkMacSystemFont,Arial,sans-serif;
                               font-size:11px;color:#444444;margin:0;"">
                      &copy; 2026
                    </p>
                  </td>
                </tr>
              </table>
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
