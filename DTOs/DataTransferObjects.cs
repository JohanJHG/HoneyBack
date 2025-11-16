using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs
{
    // DTOs para Usuario
    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(255, ErrorMessage = "El nombre no puede exceder 255 caracteres")]
        public string NombreCompleto { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = null!;
    }

    public class UsuarioUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string NombreCompleto { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }
    }

    public class UsuarioResponseDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
    }

    // DTOs para Reporte
    public class ReporteCreateDto
    {
        [Required(ErrorMessage = "El nombre del reporte es requerido")]
        [StringLength(255)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de reporte es requerido")]
        [StringLength(50)]
        public string TipoReporte { get; set; } = null!;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UsuarioId { get; set; }
    }

    public class ReporteUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TipoReporte { get; set; } = null!;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";
    }

    // DTOs para Sesión
    public class SesionCreateDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(500)]
        public string TokenSesion { get; set; } = null!;

        [Required]
        public DateTime FechaExpiracion { get; set; }
    }

    // DTOs para Mensaje de Contacto
    public class MensajeContactoCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(255)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 2000 caracteres")]
        public string Mensaje { get; set; } = null!;
    }

    // DTO genérico para respuestas
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}
