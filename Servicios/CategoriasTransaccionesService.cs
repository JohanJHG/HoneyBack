using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class CategoriasTransaccionesService : ICategoriasTransaccionesService
    {
        private readonly HoneyBalanceDbContext _context;

        public CategoriasTransaccionesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoriasTransaccione>> ObtenerTodosAsync()
        {
            return await _context.CategoriasTransacciones
                .Include(c => c.Usuario)
                .ToListAsync();
        }

        public async Task<CategoriasTransaccione?> ObtenerPorIdAsync(int id)
        {
            return await _context.CategoriasTransacciones
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);
        }

        public async Task<IEnumerable<CategoriasTransaccione>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.CategoriasTransacciones
                .Where(c => c.UsuarioId == usuarioId || c.EsSistema == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoAsync(string tipo)
        {
            return await _context.CategoriasTransacciones
                .Where(c => c.Tipo == tipo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene categorías filtradas por tipo y usuario (incluye las de sistema)
        /// </summary>
        public async Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoYUsuarioAsync(string tipo, int usuarioId)
        {
            return await _context.CategoriasTransacciones
                .Where(c => c.Tipo == tipo && (c.UsuarioId == usuarioId || c.EsSistema == true))
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoriasTransaccione>> ObtenerCategoriasActivasAsync(int usuarioId)
        {
            return await _context.CategoriasTransacciones
                .Where(c => (c.UsuarioId == usuarioId || c.EsSistema == true) && c.Activa == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<CategoriasTransaccione> CrearAsync(CategoriasTransaccione categoria)
        {
            _context.CategoriasTransacciones.Add(categoria);
            await _context.SaveChangesAsync();
            return categoria;
        }

        public async Task<CategoriasTransaccione?> ActualizarAsync(int id, CategoriasTransaccione categoria)
        {
            var categoriaExistente = await _context.CategoriasTransacciones.FindAsync(id);
            if (categoriaExistente == null)
                return null;

            categoriaExistente.Nombre = categoria.Nombre;
            categoriaExistente.Tipo = categoria.Tipo;
            categoriaExistente.Color = categoria.Color;
            categoriaExistente.Icono = categoria.Icono;
            categoriaExistente.Activa = categoria.Activa;

            await _context.SaveChangesAsync();
            return categoriaExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var categoria = await _context.CategoriasTransacciones.FindAsync(id);
            if (categoria == null || categoria.EsSistema == true)
                return false;

            _context.CategoriasTransacciones.Remove(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int usuarioId, string tipo)
        {
            return await _context.CategoriasTransacciones
                .AnyAsync(c => c.Nombre == nombre && c.UsuarioId == usuarioId && c.Tipo == tipo);
        }
    }
}
