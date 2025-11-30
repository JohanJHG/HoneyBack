using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IConfiguracionesUsuarioService
    {
        Task<IEnumerable<ConfiguracionesUsuario>> ObtenerTodosAsync();
        Task<ConfiguracionesUsuario?> ObtenerPorIdAsync(int id);
        Task<ConfiguracionesUsuario?> ObtenerPorUsuarioIdAsync(int usuarioId);
        Task<ConfiguracionesUsuario> CrearAsync(ConfiguracionesUsuario configuracion);
        Task<ConfiguracionesUsuario?> ActualizarAsync(int id, ConfiguracionesUsuario configuracion);
        Task<bool> EliminarAsync(int id);
        Task<bool> MarcarComoVeterano(int usuarioId);
    }
}
