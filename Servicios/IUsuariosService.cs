using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IUsuariosService
    {
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task<Usuario?> ActualizarAsync(int id, Usuario usuario);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteEmailAsync(string email);
    }
}
