using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class MensajesContactoService : IMensajesContactoService
    {
        private readonly HoneyBalanceDbContext _context;

        public MensajesContactoService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MensajesContacto>> ObtenerTodosAsync()
        {
            return await _context.MensajesContactos
                .OrderByDescending(m => m.FechaEnvio)
                .ToListAsync();
        }

        public async Task<MensajesContacto?> ObtenerPorIdAsync(int id)
        {
            return await _context.MensajesContactos.FindAsync(id);
        }

        public async Task<MensajesContacto> CrearAsync(MensajesContacto mensaje)
        {
            mensaje.FechaEnvio = DateTime.Now;
            _context.MensajesContactos.Add(mensaje);
            await _context.SaveChangesAsync();
            return mensaje;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var mensaje = await _context.MensajesContactos.FindAsync(id);
            if (mensaje == null)
                return false;

            _context.MensajesContactos.Remove(mensaje);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
