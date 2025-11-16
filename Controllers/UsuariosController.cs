using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerTodos()
        {
            try
            {
                var usuarios = await _usuariosService.ObtenerTodosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuarios", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> ObtenerPorId(int id)
        {
            try
            {
                var usuario = await _usuariosService.ObtenerPorIdAsync(id);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<Usuario>> ObtenerPorEmail(string email)
        {
            try
            {
                var usuario = await _usuariosService.ObtenerPorEmailAsync(email);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Crear([FromBody] Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _usuariosService.ExisteEmailAsync(usuario.Email))
                    return Conflict(new { mensaje = "El email ya está registrado" });

                var nuevoUsuario = await _usuariosService.CrearAsync(usuario);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoUsuario.UsuarioId }, nuevoUsuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear usuario", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Usuario>> Actualizar(int id, [FromBody] Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioActualizado = await _usuariosService.ActualizarAsync(id, usuario);
                if (usuarioActualizado == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(usuarioActualizado);
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
