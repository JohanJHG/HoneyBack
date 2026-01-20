using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.DTOs;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// NOTA: El endpoint GET global de todos los usuarios debería estar restringido a administradores
        /// Por seguridad, este endpoint ahora retorna solo el perfil del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerMiPerfil()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var usuario = await _usuariosService.ObtenerPorIdAsync(userId.Value);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                var response = new UsuarioResponseDto
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede ver su propio perfil
                if (id != userId.Value)
                    return Forbid();

                var usuario = await _usuariosService.ObtenerPorIdAsync(id);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                var response = new UsuarioResponseDto
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorEmail(string email)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var usuario = await _usuariosService.ObtenerPorEmailAsync(email);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Solo puede ver su propio perfil
                if (usuario.UsuarioId != userId.Value)
                    return Forbid();

                var response = new UsuarioResponseDto
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> Actualizar(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede actualizar su propio perfil
                if (id != userId.Value)
                    return Forbid();

                var usuario = new Usuario
                {
                    NombreCompleto = usuarioDto.NombreCompleto,
                    Email = usuarioDto.Email,
                    PasswordHash = string.Empty // No actualizar password desde este endpoint
                };

                var usuarioActualizado = await _usuariosService.ActualizarAsync(id, usuario);
                if (usuarioActualizado == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                var response = new UsuarioResponseDto
                {
                    UsuarioId = usuarioActualizado.UsuarioId,
                    NombreCompleto = usuarioActualizado.NombreCompleto,
                    Email = usuarioActualizado.Email,
                    FechaRegistro = usuarioActualizado.FechaRegistro
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar usuario", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede eliminar su propio perfil
                if (id != userId.Value)
                    return Forbid();

                var resultado = await _usuariosService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(new { mensaje = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar usuario", error = ex.Message });
            }
        }
    }
}
