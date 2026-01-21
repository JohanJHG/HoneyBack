using System;

namespace HoneyBack.Models;

public partial class Transaccione
{
    public int TransaccionId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Monto { get; set; }

    public string Tipo { get; set; } = null!; // "ingreso" o "gasto"

    public DateOnly Fecha { get; set; }

    public string? Categoria { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
