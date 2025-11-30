using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class Sesione
{
    public long SesionId { get; set; }

    public int UsuarioId { get; set; }

    public string TokenSesion { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public string? Ipaddress { get; set; }

    public string? UserAgent { get; set; }

    public bool? Activa { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
