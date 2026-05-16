using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth
{
    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres")]
        public string PasswordActual { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        [MaxLength(100, ErrorMessage = "La nueva contraseña no puede exceder 100 caracteres")]
        public string PasswordNueva { get; set; } = null!;

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña")]
        [MaxLength(100)]
        public string ConfirmarPasswordNueva { get; set; } = null!;
    }
}
