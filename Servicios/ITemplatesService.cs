using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface ITemplatesService
    {
        Task<IEnumerable<Template>> ObtenerTodosAsync();
        Task<Template?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Template>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Template>> ObtenerActivosPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Template>> ObtenerMasUsadosAsync(int usuarioId, int cantidad = 10);
        Task<Template> CrearAsync(Template template);
        Task<Template?> ActualizarAsync(int id, Template template);
        Task<bool> EliminarAsync(int id);
        Task<bool> RegistrarUsoAsync(int id);
        Task<bool> ExisteNombreAsync(string nombre, int usuarioId);
    }
}
