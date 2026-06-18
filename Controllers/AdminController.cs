using HoneyBack.DTOs.Admin;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IEmailService _emailService;

        public AdminController(IAdminService adminService, IEmailService emailService)
        {
            _adminService = adminService;
            _emailService = emailService;
        }

        [HttpGet("usuarios")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<UsuariosPageDto>> ObtenerUsuarios(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var resultado = await _adminService.ObtenerUsuariosPaginadoAsync(page, pageSize, search, status);
            return Ok(resultado);
        }

        [HttpGet("usuarios/{id:int}")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<UsuarioAdminDto>> ObtenerUsuarioPorId(int id)
        {
            var usuario = await _adminService.ObtenerUsuarioPorIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(usuario);
        }

        [HttpPatch("usuarios/{id:int}/rol")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult<UsuarioAdminDto>> CambiarRol(int id, [FromBody] CambiarRolDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _adminService.CambiarRolAsync(id, dto.NuevoRol);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet("roles/config")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public ActionResult<List<RolConfigDto>> ObtenerConfigRoles()
        {
            return Ok(_adminService.ObtenerConfigRoles());
        }

        // ── Mensajes de contacto ──────────────────────────────────────────────

        [HttpGet("mensajes")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<MensajesPageDto>> ObtenerMensajes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? leido = null,
            [FromQuery] bool? respondido = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var resultado = await _adminService.ObtenerMensajesPaginadoAsync(page, pageSize, leido, respondido);
            return Ok(resultado);
        }

        [HttpGet("mensajes/{id:int}")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<MensajeAdminDto>> ObtenerMensajePorId(int id)
        {
            var mensaje = await _adminService.ObtenerMensajePorIdAsync(id);
            if (mensaje == null)
                return NotFound(new { mensaje = "Mensaje no encontrado" });

            return Ok(mensaje);
        }

        [HttpPatch("mensajes/{id:int}/estado")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<MensajeAdminDto>> CambiarEstadoMensaje(int id, [FromBody] CambiarEstadoMensajeDto dto)
        {
            try
            {
                var resultado = await _adminService.MarcarLeidoAsync(id, dto.Leido);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("mensajes/{id:int}/responder")]
        [Authorize(Policy = "AdminOrSuperAdmin")]
        public async Task<ActionResult<MensajeAdminDto>> ResponderMensaje(int id, [FromBody] ResponderMensajeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _adminService.ResponderMensajeAsync(id, dto.Asunto, dto.Cuerpo, _emailService);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}
