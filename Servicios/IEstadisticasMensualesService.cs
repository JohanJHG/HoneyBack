using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IEstadisticasMensualesService
    {
        Task<IEnumerable<EstadisticasMensuale>> ObtenerTodosAsync();
        Task<EstadisticasMensuale?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<EstadisticasMensuale>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<EstadisticasMensuale?> ObtenerPorPeriodoAsync(int usuarioId, int anio, int mes);
        Task<IEnumerable<EstadisticasMensuale>> ObtenerPorAnioAsync(int usuarioId, int anio);
        Task<EstadisticasMensuale> CrearAsync(EstadisticasMensuale estadistica);
        Task<EstadisticasMensuale?> ActualizarAsync(int id, EstadisticasMensuale estadistica);
        Task<bool> EliminarAsync(int id);
        Task<bool> RecalcularEstadisticaAsync(int usuarioId, int anio, int mes);
    }
}
