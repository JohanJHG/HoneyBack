using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class ConfiguracionesUsuario
{
    public int ConfiguracionId { get; set; }

    public int UsuarioId { get; set; }

    public bool? NotificacionesEmail { get; set; }

    public bool? NotificacionesPush { get; set; }

    public string? Tema { get; set; }

    public string? Idioma { get; set; }

    public string? Timezone { get; set; }

    public string? FormatoFecha { get; set; }

    public bool? PrimeraVez { get; set; }

    public string? ConfiguracionPersonalizada { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    /// <summary>
    /// Moneda preferida del usuario (COP, USD, EUR, etc.)
    /// </summary>
    public string? MonedaPreferida { get; set; }

    /// <summary>
    /// Nombre de display del usuario
    /// </summary>
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// URL o base64 del avatar del usuario
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Indica si el usuario es veterano (lleva tiempo usando la app)
    /// </summary>
    public bool? EsVeterano { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
