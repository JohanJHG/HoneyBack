using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class MetasAhorro
{
    public int MetaId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoria de la meta: ahorro, vivienda, vacaciones, educacion, vehiculo, emergencia, tecnologia, otro
    /// </summary>
    public string Categoria { get; set; } = "otro";

    public decimal MontoObjetivo { get; set; }

    public decimal? MontoActual { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaObjetivo { get; set; }

    public DateTime? FechaCompletada { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public int? Prioridad { get; set; }

    public bool? Activa { get; set; }

    public bool? Completada { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
