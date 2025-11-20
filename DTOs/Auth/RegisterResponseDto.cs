namespace HoneyBack.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
    }
}
