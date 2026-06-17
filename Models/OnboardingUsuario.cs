using System;

namespace HoneyBack.Models;

public partial class OnboardingUsuario
{
    public int OnboardingId { get; set; }

    public int UsuarioId { get; set; }

    public bool Dismissed { get; set; }

    public DateTime? DismissedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
