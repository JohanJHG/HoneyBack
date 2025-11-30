using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class Reporte
{
    public int ReporteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string TipoReporte { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime FechaReporte { get; set; }

    public string Estado { get; set; } = null!;

    public int? UsuarioId { get; set; }

    public string? Parametros { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? ArchivoUrl { get; set; }

    public DateTime? FechaGeneracion { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
