using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class EstadisticasMensuale
{
    public int EstadisticaId { get; set; }

    public int UsuarioId { get; set; }

    public int Anio { get; set; }

    public int Mes { get; set; }

    public decimal? TotalIngresos { get; set; }

    public decimal? TotalGastos { get; set; }

    public decimal? Balance { get; set; }

    public int? NumTransacciones { get; set; }

    public int? CategoriaMayorGastoId { get; set; }

    public decimal? MontoMayorGasto { get; set; }

    public DateTime? FechaCalculo { get; set; }

    public virtual CategoriasTransaccione? CategoriaMayorGasto { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
