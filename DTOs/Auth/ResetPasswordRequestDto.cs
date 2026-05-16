using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "El código de verificación es requerido")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener exactamente 6 dígitos")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe contener solo dígitos")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres")]
    public string NewPassword { get; set; } = null!;
}
