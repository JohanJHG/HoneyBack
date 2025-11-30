using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public string? Telefono { get; set; }

    public DateTime? FechaUltimaActualizacion { get; set; }

    public bool? Activo { get; set; }

    public string? PreferenciasMoneda { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<CategoriasTransaccione> CategoriasTransacciones { get; set; } = new List<CategoriasTransaccione>();

    public virtual ConfiguracionesUsuario? ConfiguracionesUsuario { get; set; }

    public virtual ICollection<EstadisticasMensuale> EstadisticasMensuales { get; set; } = new List<EstadisticasMensuale>();

    public virtual ICollection<MensajesContacto> MensajesContactos { get; set; } = new List<MensajesContacto>();

    public virtual ICollection<MetasAhorro> MetasAhorros { get; set; } = new List<MetasAhorro>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual ICollection<Sesione> Sesiones { get; set; } = new List<Sesione>();

    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();
}
