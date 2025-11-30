using System;
using System.Collections.Generic;

namespace HoneyBack.Models;

public partial class MensajesContacto
{
    public int ContactoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public string? Asunto { get; set; }

    public int? UsuarioId { get; set; }

    public bool? Leido { get; set; }

    public DateTime? FechaLeido { get; set; }

    public string? Respuesta { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
