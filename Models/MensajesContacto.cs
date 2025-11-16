using System;

namespace HoneyBack.Models
{
    public partial class MensajesContacto
    {
        public int ContactoId { get; set; }

        public string Nombre { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Mensaje { get; set; } = null!;

        public DateTime FechaEnvio { get; set; }
    }
}
