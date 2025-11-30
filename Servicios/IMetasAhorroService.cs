using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IMetasAhorroService
    {
        Task<IEnumerable<MetasAhorro>> ObtenerTodosAsync();
        Task<MetasAhorro?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<MetasAhorro>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<MetasAhorro>> ObtenerActivasPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<MetasAhorro>> ObtenerCompletadasPorUsuarioAsync(int usuarioId);
        Task<MetasAhorro> CrearAsync(MetasAhorro meta);
        Task<MetasAhorro?> ActualizarAsync(int id, MetasAhorro meta);
        Task<MetasAhorro?> ActualizarMontoAsync(int id, decimal nuevoMonto);
        Task<bool> EliminarAsync(int id);
        Task<bool> MarcarComoCompletadaAsync(int id);
    }
}
