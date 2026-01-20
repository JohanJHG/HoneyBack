namespace HoneyBack.Servicios;

/// <summary>
/// Servicio para envío de emails transaccionales.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un email con el código de recuperación de contraseña.
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="userName">Nombre del usuario para personalizar el email</param>
    /// <param name="token">Código de 6 dígitos para recuperación</param>
    /// <returns>True si el email fue enviado exitosamente</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token);
}
