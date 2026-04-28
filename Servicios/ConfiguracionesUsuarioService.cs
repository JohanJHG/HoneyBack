using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class ConfiguracionesUsuarioService : IConfiguracionesUsuarioService
    {
        private readonly HoneyBalanceDbContext _context;

        public ConfiguracionesUsuarioService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConfiguracionesUsuario>> ObtenerTodosAsync()
        {
            return await _context.ConfiguracionesUsuarios
                .Include(c => c.Usuario)
                .ToListAsync();
        }

        public async Task<ConfiguracionesUsuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.ConfiguracionesUsuarios
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.ConfiguracionId == id);
        }

        public async Task<ConfiguracionesUsuario?> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.ConfiguracionesUsuarios
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        public async Task<ConfiguracionesUsuario> CrearAsync(ConfiguracionesUsuario configuracion)
        {
            configuracion.FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            configuracion.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            configuracion.NotificacionesEmail ??= true;
            configuracion.NotificacionesPush ??= true;
            configuracion.Tema ??= "dark";
            configuracion.Idioma ??= "es";
            configuracion.Timezone ??= "America/Bogota";
            configuracion.FormatoFecha ??= "DD/MM/YYYY";
            configuracion.PrimeraVez ??= true;
            configuracion.MonedaPreferida ??= "COP";
            configuracion.EsVeterano ??= false;

            _context.ConfiguracionesUsuarios.Add(configuracion);
            await _context.SaveChangesAsync();
            return configuracion;
        }

        public async Task<ConfiguracionesUsuario?> ActualizarAsync(int id, ConfiguracionesUsuario configuracion)
        {
            var configuracionExistente = await _context.ConfiguracionesUsuarios.FindAsync(id);
            if (configuracionExistente == null)
                return null;

            configuracionExistente.NotificacionesEmail = configuracion.NotificacionesEmail ?? configuracionExistente.NotificacionesEmail;
            configuracionExistente.NotificacionesPush = configuracion.NotificacionesPush ?? configuracionExistente.NotificacionesPush;
            configuracionExistente.Tema = configuracion.Tema ?? configuracionExistente.Tema;
            configuracionExistente.Idioma = configuracion.Idioma ?? configuracionExistente.Idioma;
            configuracionExistente.Timezone = configuracion.Timezone ?? configuracionExistente.Timezone;
            configuracionExistente.FormatoFecha = configuracion.FormatoFecha ?? configuracionExistente.FormatoFecha;
            configuracionExistente.PrimeraVez = configuracion.PrimeraVez ?? configuracionExistente.PrimeraVez;
            configuracionExistente.ConfiguracionPersonalizada = configuracion.ConfiguracionPersonalizada ?? configuracionExistente.ConfiguracionPersonalizada;
            configuracionExistente.MonedaPreferida = configuracion.MonedaPreferida ?? configuracionExistente.MonedaPreferida;
            configuracionExistente.NombreUsuario = configuracion.NombreUsuario ?? configuracionExistente.NombreUsuario;
            configuracionExistente.AvatarUrl = configuracion.AvatarUrl ?? configuracionExistente.AvatarUrl;
            configuracionExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return configuracionExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var configuracion = await _context.ConfiguracionesUsuarios.FindAsync(id);
            if (configuracion == null)
                return false;

            _context.ConfiguracionesUsuarios.Remove(configuracion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarcarComoVeterano(int usuarioId)
        {
            var configuracion = await ObtenerPorUsuarioIdAsync(usuarioId);
            if (configuracion == null)
                return false;

            configuracion.PrimeraVez = false;
            configuracion.EsVeterano = true;
            configuracion.FechaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
