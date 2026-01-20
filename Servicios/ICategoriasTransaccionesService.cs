using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface ICategoriasTransaccionesService
    {
        Task<IEnumerable<CategoriasTransaccione>> ObtenerTodosAsync();
        Task<CategoriasTransaccione?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<CategoriasTransaccione>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoAsync(string tipo);
        Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoYUsuarioAsync(string tipo, int usuarioId);
        Task<IEnumerable<CategoriasTransaccione>> ObtenerCategoriasActivasAsync(int usuarioId);
        Task<CategoriasTransaccione> CrearAsync(CategoriasTransaccione categoria);
        Task<CategoriasTransaccione?> ActualizarAsync(int id, CategoriasTransaccione categoria);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteNombreAsync(string nombre, int usuarioId, string tipo);
    }
}
