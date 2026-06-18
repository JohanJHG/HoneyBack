using HoneyBack.DTOs.Admin;
using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class AdminService : IAdminService
    {
        private readonly HoneyBalanceDbContext _context;

        public AdminService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<UsuariosPageDto> ObtenerUsuariosPaginadoAsync(int page, int pageSize, string? search, string? status)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.NombreCompleto.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bool activo = status.Equals("activo", StringComparison.OrdinalIgnoreCase);
                query = query.Where(u => u.Activo == activo);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.FechaRegistro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => MapToDto(u))
                .ToListAsync();

            return new UsuariosPageDto
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UsuarioAdminDto?> ObtenerUsuarioPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            return usuario == null ? null : MapToDto(usuario);
        }

        public async Task<UsuarioAdminDto> CambiarRolAsync(int id, string nuevoRol)
        {
            if (!Enum.TryParse<RolUsuario>(nuevoRol, true, out var rol))
                throw new ArgumentException($"Rol inválido: {nuevoRol}");

            if (rol == RolUsuario.SuperAdmin)
                throw new InvalidOperationException("No se puede asignar el rol SuperAdmin desde la API");

            var usuario = await _context.Usuarios.FindAsync(id)
                ?? throw new KeyNotFoundException($"Usuario {id} no encontrado");

            if (usuario.Rol == RolUsuario.SuperAdmin)
                throw new InvalidOperationException("No se puede modificar el rol del SuperAdmin");

            usuario.Rol = rol;
            usuario.FechaUltimaActualizacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();

            return MapToDto(usuario);
        }

        public List<RolConfigDto> ObtenerConfigRoles()
        {
            return new List<RolConfigDto>
            {
                new()
                {
                    Id = "Administrador",
                    Nombre = "Administrador",
                    Descripcion = "Acceso completo al software (personal, empresarial y panel admin, excepto gestión de roles)",
                    Permisos = new List<string> { "view_users", "view_reports", "view_logs", "manage_personal", "manage_empresarial" }
                },
                new()
                {
                    Id = "Usuario",
                    Nombre = "Usuario",
                    Descripcion = "Acceso completo a las vistas personal y empresarial",
                    Permisos = new List<string> { "manage_personal", "manage_empresarial" }
                }
            };
        }

        private static UsuarioAdminDto MapToDto(Usuario u) => new()
        {
            Id = u.UsuarioId,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            FechaRegistro = u.FechaRegistro,
            Rol = u.Rol.ToString(),
            Activo = u.Activo ?? false
        };

        // ── Mensajes ─────────────────────────────────────────────────────────

        public async Task<MensajesPageDto> ObtenerMensajesPaginadoAsync(int page, int pageSize, bool? leido, bool? respondido = null)
        {
            var query = _context.MensajesContactos.AsQueryable();

            if (leido.HasValue)
                // Leido IS NULL en DB = mensaje nunca marcado = pendiente (tratar como false)
                query = leido.Value
                    ? query.Where(m => m.Leido == true)
                    : query.Where(m => m.Leido != true);

            if (respondido.HasValue)
                query = respondido.Value
                    ? query.Where(m => m.Respuesta != null)
                    : query.Where(m => m.Respuesta == null);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.FechaEnvio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => MapMensajeToDto(m))
                .ToListAsync();

            return new MensajesPageDto { Items = items, Total = total, Page = page, PageSize = pageSize };
        }

        public async Task<MensajeAdminDto?> ObtenerMensajePorIdAsync(int id)
        {
            var m = await _context.MensajesContactos.FindAsync(id);
            return m == null ? null : MapMensajeToDto(m);
        }

        public async Task<MensajeAdminDto> MarcarLeidoAsync(int id, bool leido)
        {
            var m = await _context.MensajesContactos.FindAsync(id)
                ?? throw new KeyNotFoundException($"Mensaje {id} no encontrado");

            m.Leido = leido;
            m.FechaLeido = leido ? DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified) : null;
            await _context.SaveChangesAsync();
            return MapMensajeToDto(m);
        }

        public async Task<MensajeAdminDto> ResponderMensajeAsync(int id, string asunto, string cuerpo, IEmailService emailService)
        {
            var m = await _context.MensajesContactos.FindAsync(id)
                ?? throw new KeyNotFoundException($"Mensaje {id} no encontrado");

            await emailService.SendAdminReplyAsync(m.Email, m.Nombre, asunto, cuerpo);

            m.Respuesta = $"[Asunto: {asunto}]\n{cuerpo}";
            m.FechaRespuesta = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            m.Leido = true;
            m.FechaLeido ??= DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
            return MapMensajeToDto(m);
        }

        private static MensajeAdminDto MapMensajeToDto(Models.MensajesContacto m) => new()
        {
            Id = m.ContactoId,
            Nombre = m.Nombre,
            Email = m.Email,
            Asunto = m.Asunto,
            Mensaje = m.Mensaje,
            FechaEnvio = m.FechaEnvio,
            Leido = m.Leido ?? false,
            FechaLeido = m.FechaLeido,
            Respuesta = m.Respuesta,
            FechaRespuesta = m.FechaRespuesta,
            UsuarioId = m.UsuarioId,
        };
    }
}
