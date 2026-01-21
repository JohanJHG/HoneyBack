using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class TransaccionesService : ITransaccionesService
    {
        private readonly HoneyBalanceDbContext _context;

        public TransaccionesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaccione>> ObtenerTodosAsync()
        {
            return await _context.Transacciones
                .Include(t => t.Usuario)
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Transaccione?> ObtenerPorIdAsync(int id)
        {
            return await _context.Transacciones
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TransaccionId == id);
        }

        public async Task<IEnumerable<Transaccione>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId)
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYFechaAsync(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin)
        {
            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId && t.Fecha >= fechaInicio && t.Fecha <= fechaFin)
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYTipoAsync(int usuarioId, string tipo)
        {
            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId && t.Tipo.ToLower() == tipo.ToLower())
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaccione>> ObtenerPorUsuarioYCategoriaAsync(int usuarioId, string categoria)
        {
            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId && t.Categoria != null && t.Categoria.ToLower() == categoria.ToLower())
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Transaccione> CrearAsync(Transaccione transaccion)
        {
            transaccion.FechaCreacion = DateTime.UtcNow;
            
            // Normalizar tipo a minusculas
            transaccion.Tipo = transaccion.Tipo.ToLower();
            
            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();
            return transaccion;
        }

        public async Task<Transaccione?> ActualizarAsync(int id, Transaccione transaccion)
        {
            var transaccionExistente = await _context.Transacciones.FindAsync(id);
            if (transaccionExistente == null)
                return null;

            transaccionExistente.Nombre = transaccion.Nombre;
            transaccionExistente.Monto = transaccion.Monto;
            transaccionExistente.Tipo = transaccion.Tipo.ToLower();
            transaccionExistente.Fecha = transaccion.Fecha;
            transaccionExistente.Categoria = transaccion.Categoria;

            await _context.SaveChangesAsync();
            return transaccionExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
                return false;

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> ObtenerTotalIngresosPorUsuarioAsync(int usuarioId, int anio, int mes)
        {
            var fechaInicio = new DateOnly(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId 
                    && t.Tipo.ToLower() == "ingreso" 
                    && t.Fecha >= fechaInicio 
                    && t.Fecha <= fechaFin)
                .SumAsync(t => t.Monto);
        }

        public async Task<decimal> ObtenerTotalGastosPorUsuarioAsync(int usuarioId, int anio, int mes)
        {
            var fechaInicio = new DateOnly(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId 
                    && t.Tipo.ToLower() == "gasto" 
                    && t.Fecha >= fechaInicio 
                    && t.Fecha <= fechaFin)
                .SumAsync(t => t.Monto);
        }
    }
}
