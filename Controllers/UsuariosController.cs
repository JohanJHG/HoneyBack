using BCrypt.Net;
using HoneyBack.DTOs;
using HoneyBack.Extensions;
using HoneyBack.Models;
using HoneyBack.Servicios;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ObtenerTodos()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            // Sin roles de administrador, exponemos solo el perfil del usuario autenticado.
            var usuario = await _usuariosService.ObtenerPorIdAsync(userId.Value);
            if (usuario == null)
                return Ok(Array.Empty<UsuarioResponseDto>());

            return Ok(new[] { MapToDto(usuario) });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (id != userId.Value)
                return Forbid();

            var usuario = await _usuariosService.ObtenerPorIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(MapToDto(usuario));
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorEmail(string email)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var usuario = await _usuariosService.ObtenerPorEmailAsync(email);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (usuario.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToDto(usuario));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioResponseDto>> Crear([FromBody] UsuarioCreateDto usuarioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _usuariosService.ExisteEmailAsync(usuarioDto.Email))
                return Conflict(new { mensaje = "El email ya está registrado" });

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = usuarioDto.NombreCompleto,
                Email = usuarioDto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Password),
                FechaRegistro = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var usuarioCreado = await _usuariosService.CrearAsync(nuevoUsuario);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuarioCreado.UsuarioId }, MapToDto(usuarioCreado));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UsuarioResponseDto>> Actualizar(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (id != userId.Value)
                return Forbid();

            var existente = await _usuariosService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!string.Equals(existente.Email, usuarioDto.Email, StringComparison.OrdinalIgnoreCase) &&
                await _usuariosService.ExisteEmailAsync(usuarioDto.Email))
            {
                return Conflict(new { mensaje = "El email ya está registrado por otro usuario" });
            }

            var actualizado = new Usuario
            {
                NombreCompleto = usuarioDto.NombreCompleto,
                Email = usuarioDto.Email.Trim().ToLower(),
                PasswordHash = string.IsNullOrWhiteSpace(usuarioDto.Password)
                    ? existente.PasswordHash
                    : BCrypt.Net.BCrypt.HashPassword(usuarioDto.Password)
            };

            var usuarioActualizado = await _usuariosService.ActualizarAsync(id, actualizado);
            if (usuarioActualizado == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(MapToDto(usuarioActualizado));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (id != userId.Value)
                return Forbid();

            var eliminado = await _usuariosService.EliminarAsync(id);
            if (!eliminado)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }

        private static UsuarioResponseDto MapToDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                FechaRegistro = usuario.FechaRegistro
            };
        }
    }
}
