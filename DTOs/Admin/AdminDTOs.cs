using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Admin
{
    public class UsuarioAdminDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = null!;
        public bool Activo { get; set; }
    }

    public class UsuariosPageDto
    {
        public List<UsuarioAdminDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CambiarRolDto
    {
        [Required(ErrorMessage = "El nuevo rol es requerido")]
        public string NuevoRol { get; set; } = null!;
    }

    public class RolConfigDto
    {
        public string Id { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public List<string> Permisos { get; set; } = new();
    }

    // ── Mensajes de contacto ──────────────────────────────────────────────────

    public class MensajeAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Asunto { get; set; }
        public string Mensaje { get; set; } = null!;
        public DateTime FechaEnvio { get; set; }
        public bool Leido { get; set; }
        public DateTime? FechaLeido { get; set; }
        public string? Respuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public int? UsuarioId { get; set; }
    }

    public class MensajesPageDto
    {
        public List<MensajeAdminDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CambiarEstadoMensajeDto
    {
        [Required]
        public bool Leido { get; set; }
    }

    public class ResponderMensajeDto
    {
        [Required(ErrorMessage = "El asunto es requerido")]
        [MaxLength(300)]
        public string Asunto { get; set; } = null!;

        [Required(ErrorMessage = "El cuerpo del mensaje es requerido")]
        public string Cuerpo { get; set; } = null!;
    }
}
