using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth;

/// <summary>
/// Request para solicitar recuperación de contraseña.
/// </summary>
public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = null!;
}
