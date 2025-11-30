using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class Template
{
    public int TemplateId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public int CategoriaId { get; set; }

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public string Tipo { get; set; } = null!;

    public int? FrecuenciaUso { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaUltimoUso { get; set; }

    public bool? Activo { get; set; }

    public virtual CategoriasTransaccione Categoria { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
