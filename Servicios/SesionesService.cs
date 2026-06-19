using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class SesionesService : ISesionesService
    {
        private readonly HoneyBalanceDbContext _context;

        public SesionesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sesione>> ObtenerTodosAsync()
        {
            return await _context.Sesiones
                .Include(s => s.Usuario)
                .ToListAsync();
        }

        public async Task<Sesione?> ObtenerPorIdAsync(long id)
        {
            return await _context.Sesiones
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.SesionId == id);
        }

        public async Task<Sesione?> ObtenerPorTokenAsync(string token)
        {
            return await _context.Sesiones
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.TokenSesion == token);
        }

        public async Task<IEnumerable<Sesione>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Sesiones
                .Where(s => s.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Sesione> CrearAsync(Sesione sesion)
        {
            _context.Sesiones.Add(sesion);
            await _context.SaveChangesAsync();
            return sesion;
        }

        public async Task<Sesione?> ActualizarAsync(long id, Sesione sesion)
        {
            var sesionExistente = await _context.Sesiones.FindAsync(id);
            if (sesionExistente == null)
                return null;

            sesionExistente.TokenSesion = sesion.TokenSesion;
            sesionExistente.FechaExpiracion = sesion.FechaExpiracion;

            await _context.SaveChangesAsync();
            return sesionExistente;
        }

        public async Task<bool> EliminarAsync(long id)
        {
            var sesion = await _context.Sesiones.FindAsync(id);
            if (sesion == null)
                return false;

            _context.Sesiones.Remove(sesion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var sesion = await ObtenerPorTokenAsync(token);
            return sesion != null && sesion.FechaExpiracion > ahora;
        }

        public async Task LimpiarSesionesExpiradasAsync()
        {
            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var limite90 = ahora.AddDays(-90);

            // Soft-delete: sesiones recientes expiradas → marcar Activa=false para analytics
            var recientes = await _context.Sesiones
                .Where(s => s.FechaExpiracion < ahora && s.Activa != false && s.FechaCreacion != null)
                .ToListAsync();
            foreach (var s in recientes)
                s.Activa = false;

            // Hard-delete: sesiones muy antiguas (>90 días) o sin FechaCreacion (legacy)
            var antiguas = await _context.Sesiones
                .Where(s => s.FechaCreacion == null || s.FechaCreacion < limite90)
                .ToListAsync();
            _context.Sesiones.RemoveRange(antiguas);

            await _context.SaveChangesAsync();
        }
    }
}
