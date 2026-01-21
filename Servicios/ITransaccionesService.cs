using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface ITransaccionesService
    {
        Task<IEnumerable<Transaccione>> ObtenerTodosAsync();
        Task<Transaccione?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Transaccione>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYFechaAsync(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin);
        Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYTipoAsync(int usuarioId, string tipo);
        Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYCategoriaAsync(int usuarioId, string categoria);
        Task<Transaccione> CrearAsync(Transaccione transaccion);
        Task<Transaccione?> ActualizarAsync(int id, Transaccione transaccion);
        Task<bool> EliminarAsync(int id);
        Task<decimal> ObtenerTotalIngresosPorUsuarioAsync(int usuarioId, int anio, int mes);
        Task<decimal> ObtenerTotalGastosPorUsuarioAsync(int usuarioId, int anio, int mes);
    }
}
