using HoneyBack.DTOs.Admin;
using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IAdminService
    {
        Task<UsuariosPageDto> ObtenerUsuariosPaginadoAsync(int page, int pageSize, string? search, string? status);
        Task<UsuarioAdminDto?> ObtenerUsuarioPorIdAsync(int id);
        Task<UsuarioAdminDto> CambiarRolAsync(int id, string nuevoRol);
        List<RolConfigDto> ObtenerConfigRoles();

        Task<MensajesPageDto> ObtenerMensajesPaginadoAsync(int page, int pageSize, bool? leido, bool? respondido = null);
        Task<MensajeAdminDto?> ObtenerMensajePorIdAsync(int id);
        Task<MensajeAdminDto> MarcarLeidoAsync(int id, bool leido);
        Task<MensajeAdminDto> ResponderMensajeAsync(int id, string asunto, string cuerpo, IEmailService emailService);
    }
}
