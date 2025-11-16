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

    public virtual Usuario? Usuario { get; set; }
}
