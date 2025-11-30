using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class EstadisticasMensualesService : IEstadisticasMensualesService
    {
        private readonly HoneyBalanceDbContext _context;

        public EstadisticasMensualesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EstadisticasMensuale>> ObtenerTodosAsync()
        {
            return await _context.EstadisticasMensuales
                .Include(e => e.Usuario)
                .Include(e => e.CategoriaMayorGasto)
                .OrderByDescending(e => e.Anio)
                .ThenByDescending(e => e.Mes)
                .ToListAsync();
        }

        public async Task<EstadisticasMensuale?> ObtenerPorIdAsync(int id)
        {
            return await _context.EstadisticasMensuales
                .Include(e => e.Usuario)
                .Include(e => e.CategoriaMayorGasto)
                .FirstOrDefaultAsync(e => e.EstadisticaId == id);
        }

        public async Task<IEnumerable<EstadisticasMensuale>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.EstadisticasMensuales
                .Where(e => e.UsuarioId == usuarioId)
                .Include(e => e.CategoriaMayorGasto)
                .OrderByDescending(e => e.Anio)
                .ThenByDescending(e => e.Mes)
                .ToListAsync();
        }

        public async Task<EstadisticasMensuale?> ObtenerPorPeriodoAsync(int usuarioId, int anio, int mes)
        {
            return await _context.EstadisticasMensuales
                .Include(e => e.CategoriaMayorGasto)
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Anio == anio && e.Mes == mes);
        }

        public async Task<IEnumerable<EstadisticasMensuale>> ObtenerPorAnioAsync(int usuarioId, int anio)
        {
            return await _context.EstadisticasMensuales
                .Where(e => e.UsuarioId == usuarioId && e.Anio == anio)
                .Include(e => e.CategoriaMayorGasto)
                .OrderBy(e => e.Mes)
                .ToListAsync();
        }

        public async Task<EstadisticasMensuale> CrearAsync(EstadisticasMensuale estadistica)
        {
            estadistica.FechaCalculo = DateTime.Now;
            estadistica.NumTransacciones ??= 0;
            estadistica.TotalIngresos ??= 0;
            estadistica.TotalGastos ??= 0;

            _context.EstadisticasMensuales.Add(estadistica);
            await _context.SaveChangesAsync();
            return estadistica;
        }

        public async Task<EstadisticasMensuale?> ActualizarAsync(int id, EstadisticasMensuale estadistica)
        {
            var estadisticaExistente = await _context.EstadisticasMensuales.FindAsync(id);
            if (estadisticaExistente == null)
                return null;

            estadisticaExistente.TotalIngresos = estadistica.TotalIngresos;
            estadisticaExistente.TotalGastos = estadistica.TotalGastos;
            estadisticaExistente.NumTransacciones = estadistica.NumTransacciones;
            estadisticaExistente.CategoriaMayorGastoId = estadistica.CategoriaMayorGastoId;
            estadisticaExistente.MontoMayorGasto = estadistica.MontoMayorGasto;
            estadisticaExistente.FechaCalculo = DateTime.Now;

            await _context.SaveChangesAsync();
            return estadisticaExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var estadistica = await _context.EstadisticasMensuales.FindAsync(id);
            if (estadistica == null)
                return false;

            _context.EstadisticasMensuales.Remove(estadistica);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RecalcularEstadisticaAsync(int usuarioId, int anio, int mes)
        {
            var estadistica = await ObtenerPorPeriodoAsync(usuarioId, anio, mes);
            if (estadistica == null)
                return false;

            // Aquí iría la lógica de recálculo basada en transacciones
            // Por ahora solo actualizamos la fecha de cálculo
            estadistica.FechaCalculo = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
