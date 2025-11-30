using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class CategoriasTransaccione
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public bool? EsSistema { get; set; }

    public int? UsuarioId { get; set; }

    public bool? Activa { get; set; }

    public virtual ICollection<EstadisticasMensuale> EstadisticasMensuales { get; set; } = new List<EstadisticasMensuale>();

    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

    public virtual Usuario? Usuario { get; set; }
}
