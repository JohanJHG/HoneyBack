using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 255 caracteres")]
        public string NombreCompleto { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "El email debe tener entre 6 y 255 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "El reCAPTCHA es requerido")]
        public string RecaptchaToken { get; set; } = null!;
    }
}
