using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth;

/// <summary>
/// Request para restablecer la contraseña con el código de verificación.
/// </summary>
public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "El codigo de verificacion es requerido")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El codigo debe tener exactamente 6 digitos")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El codigo debe contener solo digitos")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "La nueva contrasena es requerida")]
    [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres")]
    [MaxLength(100, ErrorMessage = "La contrasena no puede exceder 100 caracteres")]
    public string NewPassword { get; set; } = null!;
}
