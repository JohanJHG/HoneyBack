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

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual ICollection<Sesione> Sesiones { get; set; } = new List<Sesione>();
}
