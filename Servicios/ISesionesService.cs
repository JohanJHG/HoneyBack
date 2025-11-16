using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface ISesionesService
    {
        Task<IEnumerable<Sesione>> ObtenerTodosAsync();
        Task<Sesione?> ObtenerPorIdAsync(long id);
        Task<Sesione?> ObtenerPorTokenAsync(string token);
        Task<IEnumerable<Sesione>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<Sesione> CrearAsync(Sesione sesion);
        Task<Sesione?> ActualizarAsync(long id, Sesione sesion);
        Task<bool> EliminarAsync(long id);
        Task<bool> ValidarTokenAsync(string token);
        Task LimpiarSesionesExpiradasAsync();
    }
}
