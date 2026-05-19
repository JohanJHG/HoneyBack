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
        [EmailAddress(ErrorMessage = "Email invalido")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contrasena es requerida")]
        [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres")]
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

        [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres")]
        public string? Password { get; set; }
    }

    public class UsuarioResponseDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
    }

    // DTOs para Sesion
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
        [StringLength(255, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 255 caracteres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "El email debe tener entre 6 y 255 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 2000 caracteres")]
        public string Mensaje { get; set; } = null!;
    }

    // DTOs para Transacciones
    public class TransaccionCreateDto
    {
        [Required(ErrorMessage = "El nombre de la transaccion es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999_999_999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999,999,999.99")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El tipo es requerido (ingreso/gasto)")]
        [RegularExpression("^(ingreso|gasto)$", ErrorMessage = "El tipo debe ser 'ingreso' o 'gasto'")]
        public string Tipo { get; set; } = null!;

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateOnly Fecha { get; set; }

        [StringLength(50)]
        public string? Categoria { get; set; }

        /// <summary>
        /// Se ignora - El usuarioId se toma del token JWT
        /// </summary>
        public int? UsuarioId { get; set; }
    }

    public class TransaccionUpdateDto
    {
        [StringLength(200)]
        public string? Nombre { get; set; }

        [Range(0.01, 999_999_999.99, ErrorMessage = "El monto debe estar entre 0.01 y 999,999,999.99")]
        public decimal? Monto { get; set; }

        [RegularExpression("^(ingreso|gasto)$", ErrorMessage = "El tipo debe ser 'ingreso' o 'gasto'")]
        public string? Tipo { get; set; }

        public DateOnly? Fecha { get; set; }

        [StringLength(50)]
        public string? Categoria { get; set; }
    }

    public class TransaccionResponseDto
    {
        public int TransaccionId { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = null!;
        public string Fecha { get; set; } = null!;
        public string? Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    // DTOs para MetasAhorro
    public class MetaAhorroCreateDto
    {
        [Required(ErrorMessage = "El nombre de la meta es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = null!;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Categoria: ahorro, vivienda, vacaciones, educacion, vehiculo, emergencia, tecnologia, otro
        /// </summary>
        [StringLength(50)]
        public string? Categoria { get; set; } = "otro";

        [Required(ErrorMessage = "El monto objetivo es requerido")]
        [Range(0.01, 999_999_999.99, ErrorMessage = "El monto objetivo debe estar entre 0.01 y 999,999,999.99")]
        public decimal MontoObjetivo { get; set; }

        [Range(0, 999_999_999.99, ErrorMessage = "El monto actual no puede ser negativo")]
        public decimal? MontoActual { get; set; }

        public DateOnly? FechaObjetivo { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "El color debe ser en formato hexadecimal (ej: #FFD8A9)")]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [Range(0, 10, ErrorMessage = "La prioridad debe estar entre 0 y 10")]
        public int? Prioridad { get; set; }
    }

    public class MetaAhorroUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = null!;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Categoria: ahorro, vivienda, vacaciones, educacion, vehiculo, emergencia, tecnologia, otro
        /// </summary>
        [StringLength(50)]
        public string? Categoria { get; set; }

        [Required]
        [Range(0.01, 999_999_999.99)]
        public decimal MontoObjetivo { get; set; }

        [Range(0, 999_999_999.99)]
        public decimal? MontoActual { get; set; }

        public DateOnly? FechaObjetivo { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "El color debe ser en formato hexadecimal (ej: #FFD8A9)")]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [Range(0, 10)]
        public int? Prioridad { get; set; }

        public bool? Activa { get; set; }

        public bool? Completada { get; set; }
    }

    public class MetaAhorroResponseDto
    {
        public int MetaId { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = "otro";
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaObjetivo { get; set; }
        public DateTime? FechaCompletada { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public int? Prioridad { get; set; }
        public bool? Activa { get; set; }
        public bool? Completada { get; set; }
        public decimal PorcentajeAvance => MontoObjetivo > 0 ? Math.Round((MontoActual / MontoObjetivo) * 100, 2) : 0;
    }

    // DTOs para ConfiguracionesUsuario
    public class ConfiguracionUsuarioCreateDto
    {
        [Required]
        public int UsuarioId { get; set; }

        public bool? NotificacionesEmail { get; set; } = true;

        public bool? NotificacionesPush { get; set; } = true;

        [StringLength(20)]
        public string? Tema { get; set; } = "dark";

        [StringLength(10)]
        public string? Idioma { get; set; } = "es";

        [StringLength(50)]
        public string? Timezone { get; set; } = "America/Bogota";

        [StringLength(20)]
        public string? FormatoFecha { get; set; } = "DD/MM/YYYY";

        public bool? PrimeraVez { get; set; } = true;

        [StringLength(3)]
        public string? MonedaPreferida { get; set; } = "COP";

        [StringLength(100)]
        public string? NombreUsuario { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }

    public class ConfiguracionUsuarioUpdateDto
    {
        public bool? NotificacionesEmail { get; set; }

        public bool? NotificacionesPush { get; set; }

        [StringLength(20)]
        public string? Tema { get; set; }

        [StringLength(10)]
        public string? Idioma { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        [StringLength(20)]
        public string? FormatoFecha { get; set; }

        public bool? PrimeraVez { get; set; }

        public string? ConfiguracionPersonalizada { get; set; }

        [StringLength(3)]
        public string? MonedaPreferida { get; set; }

        [StringLength(100)]
        public string? NombreUsuario { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }

    public class ConfiguracionUsuarioResponseDto
    {
        public int ConfiguracionId { get; set; }
        public int UsuarioId { get; set; }
        public bool NotificacionesEmail { get; set; }
        public bool NotificacionesPush { get; set; }
        public string Tema { get; set; } = null!;
        public string Idioma { get; set; } = null!;
        public string Timezone { get; set; } = null!;
        public string FormatoFecha { get; set; } = null!;
        public bool PrimeraVez { get; set; }
        public string? ConfiguracionPersonalizada { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? MonedaPreferida { get; set; }
        public string? NombreUsuario { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? EsVeterano { get; set; }
    }

    // DTOs para EntornosPersonales
    public class EntornoPersonalCreateDto
    {
        [Required(ErrorMessage = "El modulo es requerido")]
        [StringLength(50)]
        public string ModuloClave { get; set; } = null!;

        [Required(ErrorMessage = "El titulo es requerido")]
        [StringLength(200)]
        public string Titulo { get; set; } = null!;

        [StringLength(300)]
        public string? Subtitulo { get; set; }

        [StringLength(100)]
        public string? ValorPrincipal { get; set; }

        [StringLength(50)]
        public string? Etiqueta { get; set; }

        [Required(ErrorMessage = "Los datos son requeridos")]
        public string DatosJson { get; set; } = null!;
    }

    public class EntornoPersonalUpdateDto
    {
        [StringLength(200)]
        public string? Titulo { get; set; }

        [StringLength(300)]
        public string? Subtitulo { get; set; }

        [StringLength(100)]
        public string? ValorPrincipal { get; set; }

        [StringLength(50)]
        public string? Etiqueta { get; set; }

        public string? DatosJson { get; set; }
    }

    public class EntornoPersonalResponseDto
    {
        public int EntornoId { get; set; }
        public int UsuarioId { get; set; }
        public string ModuloClave { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string? Subtitulo { get; set; }
        public string? ValorPrincipal { get; set; }
        public string? Etiqueta { get; set; }
        public string DatosJson { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    // DTO generico para respuestas
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}
