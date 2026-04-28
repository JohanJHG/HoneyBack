using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class MetasAhorroService : IMetasAhorroService
    {
        private readonly HoneyBalanceDbContext _context;

        public MetasAhorroService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MetasAhorro>> ObtenerTodosAsync()
        {
            return await _context.MetasAhorros
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.Prioridad)
                .ThenBy(m => m.FechaObjetivo)
                .ToListAsync();
        }

        public async Task<MetasAhorro?> ObtenerPorIdAsync(int id)
        {
            return await _context.MetasAhorros
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.MetaId == id);
        }

        public async Task<IEnumerable<MetasAhorro>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.MetasAhorros
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.Prioridad)
                .ThenBy(m => m.FechaObjetivo)
                .ToListAsync();
        }

        public async Task<IEnumerable<MetasAhorro>> ObtenerActivasPorUsuarioAsync(int usuarioId)
        {
            return await _context.MetasAhorros
                .Where(m => m.UsuarioId == usuarioId && m.Activa == true && m.Completada == false)
                .OrderByDescending(m => m.Prioridad)
                .ThenBy(m => m.FechaObjetivo)
                .ToListAsync();
        }

        public async Task<IEnumerable<MetasAhorro>> ObtenerCompletadasPorUsuarioAsync(int usuarioId)
        {
            return await _context.MetasAhorros
                .Where(m => m.UsuarioId == usuarioId && m.Completada == true)
                .OrderByDescending(m => m.FechaCompletada)
                .ToListAsync();
        }

        public async Task<MetasAhorro> CrearAsync(MetasAhorro meta)
        {
            meta.FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow);
            meta.MontoActual ??= 0;
            meta.Activa ??= true;
            meta.Completada = false;
            
            _context.MetasAhorros.Add(meta);
            await _context.SaveChangesAsync();
            return meta;
        }

        public async Task<MetasAhorro?> ActualizarAsync(int id, MetasAhorro meta)
        {
            var metaExistente = await _context.MetasAhorros.FindAsync(id);
            if (metaExistente == null)
                return null;

            metaExistente.Nombre = meta.Nombre;
            metaExistente.Descripcion = meta.Descripcion;
            metaExistente.MontoObjetivo = meta.MontoObjetivo;
            metaExistente.MontoActual = meta.MontoActual;
            metaExistente.FechaObjetivo = meta.FechaObjetivo;
            metaExistente.Color = meta.Color;
            metaExistente.Icono = meta.Icono;
            metaExistente.Prioridad = meta.Prioridad;
            metaExistente.Activa = meta.Activa;
            metaExistente.Completada = meta.Completada;

            // Auto-marcar como completada si se alcanz� el objetivo
            if (metaExistente.MontoActual >= metaExistente.MontoObjetivo && metaExistente.Completada == false)
            {
                metaExistente.Completada = true;
                metaExistente.FechaCompletada = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return metaExistente;
        }

        public async Task<MetasAhorro?> ActualizarMontoAsync(int id, decimal nuevoMonto)
        {
            var meta = await _context.MetasAhorros.FindAsync(id);
            if (meta == null)
                return null;

            meta.MontoActual = nuevoMonto;

            // Auto-marcar como completada si se alcanz� el objetivo
            if (meta.MontoActual >= meta.MontoObjetivo && meta.Completada == false)
            {
                meta.Completada = true;
                meta.FechaCompletada = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return meta;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var meta = await _context.MetasAhorros.FindAsync(id);
            if (meta == null)
                return false;

            _context.MetasAhorros.Remove(meta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarcarComoCompletadaAsync(int id)
        {
            var meta = await _context.MetasAhorros.FindAsync(id);
            if (meta == null)
                return false;

            meta.Completada = true;
            meta.FechaCompletada = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
