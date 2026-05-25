using HoneyBack.Models;

namespace HoneyBack.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string ExpiresAt { get; set; } = null!; // ISO 8601
        public string RefreshToken { get; set; } = null!;
        public UsuarioDto Usuario { get; set; } = null!;
    }

    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = null!;
    }
}
