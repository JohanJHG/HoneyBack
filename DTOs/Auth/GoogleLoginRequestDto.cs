using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth
{
    public class GoogleLoginRequestDto
    {
        [Required(ErrorMessage = "El token de Google es requerido")]
        public string IdToken { get; set; } = null!;
    }
}
