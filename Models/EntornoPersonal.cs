using System;

namespace HoneyBack.Models;

public partial class EntornoPersonal
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

    public virtual Usuario Usuario { get; set; } = null!;
}
