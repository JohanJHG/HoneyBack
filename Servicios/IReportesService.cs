using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IReportesService
    {
        Task<IEnumerable<Reporte>> ObtenerTodosAsync();
        Task<Reporte?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Reporte>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Reporte>> ObtenerPorEstadoAsync(string estado);
        Task<Reporte> CrearAsync(Reporte reporte);
        Task<Reporte?> ActualizarAsync(int id, Reporte reporte);
        Task<bool> EliminarAsync(int id);
    }
}
