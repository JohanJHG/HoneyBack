using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IEntornosPersonalesService
    {
        Task<IEnumerable<EntornoPersonal>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<EntornoPersonal>> ObtenerPorUsuarioYModuloAsync(int usuarioId, string moduloClave);
        Task<EntornoPersonal?> ObtenerPorIdAsync(int id);
        Task<EntornoPersonal> CrearAsync(EntornoPersonal entorno);
        Task<EntornoPersonal?> ActualizarAsync(int id, EntornoPersonal entorno);
        Task<bool> EliminarAsync(int id);
        Task<int> EliminarTodosPorModuloAsync(int usuarioId, string moduloClave);
        Task<int> ContarPorModuloAsync(int usuarioId, string moduloClave);
    }
}
