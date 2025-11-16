using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IMensajesContactoService
    {
        Task<IEnumerable<MensajesContacto>> ObtenerTodosAsync();
        Task<MensajesContacto?> ObtenerPorIdAsync(int id);
        Task<MensajesContacto> CrearAsync(MensajesContacto mensaje);
        Task<bool> EliminarAsync(int id);
    }
}
