using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class TemplatesService : ITemplatesService
    {
        private readonly HoneyBalanceDbContext _context;

        public TemplatesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Template>> ObtenerTodosAsync()
        {
            return await _context.Templates
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FrecuenciaUso)
                .ToListAsync();
        }

        public async Task<Template?> ObtenerPorIdAsync(int id)
        {
            return await _context.Templates
                .Include(t => t.Usuario)
                .Include(t => t.Categoria)
                .FirstOrDefaultAsync(t => t.TemplateId == id);
        }

        public async Task<IEnumerable<Template>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Templates
                .Where(t => t.UsuarioId == usuarioId)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FrecuenciaUso)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> ObtenerActivosPorUsuarioAsync(int usuarioId)
        {
            return await _context.Templates
                .Where(t => t.UsuarioId == usuarioId && t.Activo == true)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FrecuenciaUso)
                .ToListAsync();
        }

        public async Task<IEnumerable<Template>> ObtenerMasUsadosAsync(int usuarioId, int cantidad = 10)
        {
            return await _context.Templates
                .Where(t => t.UsuarioId == usuarioId && t.Activo == true)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FrecuenciaUso)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<Template> CrearAsync(Template template)
        {
            template.FechaCreacion = DateTime.Now;
            template.FrecuenciaUso = 0;
            template.Activo ??= true;

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<Template?> ActualizarAsync(int id, Template template)
        {
            var templateExistente = await _context.Templates.FindAsync(id);
            if (templateExistente == null)
                return null;

            templateExistente.Nombre = template.Nombre;
            templateExistente.CategoriaId = template.CategoriaId;
            templateExistente.Monto = template.Monto;
            templateExistente.Descripcion = template.Descripcion;
            templateExistente.Tipo = template.Tipo;
            templateExistente.Activo = template.Activo;

            await _context.SaveChangesAsync();
            return templateExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return false;

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarUsoAsync(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return false;

            template.FrecuenciaUso = (template.FrecuenciaUso ?? 0) + 1;
            template.FechaUltimoUso = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int usuarioId)
        {
            return await _context.Templates
                .AnyAsync(t => t.Nombre == nombre && t.UsuarioId == usuarioId);
        }
    }
}
