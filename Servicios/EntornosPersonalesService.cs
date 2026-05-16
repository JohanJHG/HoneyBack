using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class EntornosPersonalesService : IEntornosPersonalesService
    {
        private readonly HoneyBalanceDbContext _context;

        public EntornosPersonalesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EntornoPersonal>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.EntornosPersonales
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.FechaActualizacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<EntornoPersonal>> ObtenerPorUsuarioYModuloAsync(int usuarioId, string moduloClave)
        {
            return await _context.EntornosPersonales
                .Where(e => e.UsuarioId == usuarioId && e.ModuloClave == moduloClave.ToLower())
                .OrderByDescending(e => e.FechaActualizacion)
                .ToListAsync();
        }

        public async Task<EntornoPersonal?> ObtenerPorIdAsync(int id)
        {
            return await _context.EntornosPersonales
                .FirstOrDefaultAsync(e => e.EntornoId == id);
        }

        public async Task<EntornoPersonal> CrearAsync(EntornoPersonal entorno)
        {
            entorno.ModuloClave = entorno.ModuloClave.ToLower();
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            entorno.FechaCreacion = now;
            entorno.FechaActualizacion = now;

            _context.EntornosPersonales.Add(entorno);
            await _context.SaveChangesAsync();
            return entorno;
        }

        public async Task<EntornoPersonal?> ActualizarAsync(int id, EntornoPersonal entorno)
        {
            var existente = await _context.EntornosPersonales.FindAsync(id);
            if (existente == null)
                return null;

            existente.Titulo = entorno.Titulo;
            existente.Subtitulo = entorno.Subtitulo;
            existente.ValorPrincipal = entorno.ValorPrincipal;
            existente.Etiqueta = entorno.Etiqueta;
            existente.DatosJson = entorno.DatosJson;
            existente.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var entorno = await _context.EntornosPersonales.FindAsync(id);
            if (entorno == null)
                return false;

            _context.EntornosPersonales.Remove(entorno);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> EliminarTodosPorModuloAsync(int usuarioId, string moduloClave)
        {
            return await _context.EntornosPersonales
                .Where(e => e.UsuarioId == usuarioId && e.ModuloClave == moduloClave.ToLower())
                .ExecuteDeleteAsync();
        }

        public async Task<int> ContarPorModuloAsync(int usuarioId, string moduloClave)
        {
            return await _context.EntornosPersonales
                .CountAsync(e => e.UsuarioId == usuarioId && e.ModuloClave == moduloClave.ToLower());
        }
    }
}
