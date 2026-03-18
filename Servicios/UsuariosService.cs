using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly HoneyBalanceDbContext _context;

        public UsuariosService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Sesiones)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Sesiones)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            usuario.FechaRegistro = DateTime.Now;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> ActualizarAsync(int id, Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
                return null;

            usuarioExistente.NombreCompleto = usuario.NombreCompleto;
            usuarioExistente.Email = usuario.Email;
            
            if (!string.IsNullOrEmpty(usuario.PasswordHash))
                usuarioExistente.PasswordHash = usuario.PasswordHash;

            await _context.SaveChangesAsync();
            return usuarioExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }
    }
}