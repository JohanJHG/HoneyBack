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

    public virtual Usuario Usuario { get; set; } = null!;
}
