using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Proteger todos los endpoints de usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ObtenerTodos()
        {
            try
            {
                var usuarios = await _usuariosService.ObtenerTodosAsync();
                
                // Mapear a DTO sin PasswordHash
                var response = usuarios.Select(u => new UsuarioResponseDto
                {
                    UsuarioId = u.UsuarioId,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    FechaRegistro = u.FechaRegistro
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuarios", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var usuario = await _usuariosService.ObtenerPorIdAsync(id);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Mapear a DTO sin PasswordHash
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
                var usuario = await _usuariosService.ObtenerPorEmailAsync(email);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Mapear a DTO sin PasswordHash
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

                var usuario = new Usuario
                {
                    NombreCompleto = usuarioDto.NombreCompleto,
                    Email = usuarioDto.Email,
                    PasswordHash = string.Empty // No actualizar password desde este endpoint
                };

                var usuarioActualizado = await _usuariosService.ActualizarAsync(id, usuario);
                if (usuarioActualizado == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Mapear a DTO sin PasswordHash
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
