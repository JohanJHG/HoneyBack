using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Autorización global, con excepciones específicas
    public class MensajesContactoController : ControllerBase
    {
        private readonly IMensajesContactoService _mensajesService;

        public MensajesContactoController(IMensajesContactoService mensajesService)
        {
            _mensajesService = mensajesService;
        }

        /// <summary>
        /// Obtiene todos los mensajes (solo administradores deberían usar esto)
        /// En una implementación completa, restringir a roles de admin
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MensajesContacto>>> ObtenerTodos()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // NOTA: Este endpoint debería estar restringido a administradores
                // Por ahora solo retorna mensajes del usuario autenticado
                var mensajes = await _mensajesService.ObtenerTodosAsync();
                var mensajesUsuario = mensajes.Where(m => m.UsuarioId == userId.Value);
                return Ok(mensajesUsuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener mensajes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MensajesContacto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var mensaje = await _mensajesService.ObtenerPorIdAsync(id);
                if (mensaje == null)
                    return NotFound(new { mensaje = "Mensaje no encontrado" });

                // Validación de propiedad (si tiene usuarioId asociado)
                if (mensaje.UsuarioId.HasValue && mensaje.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(mensaje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener mensaje", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear mensaje de contacto - Permitido para usuarios no autenticados (formulario de contacto)
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // Permitir mensajes de visitantes no autenticados
        public async Task<ActionResult<MensajesContacto>> Crear([FromBody] MensajesContacto mensaje)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Si hay usuario autenticado, asociar el mensaje
                var userId = User.GetUserId();
                if (userId.HasValue)
                {
                    mensaje.UsuarioId = userId.Value;
                }

                var nuevoMensaje = await _mensajesService.CrearAsync(mensaje);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoMensaje.ContactoId }, nuevoMensaje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear mensaje", error = ex.Message });
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

                // Validar propiedad antes de eliminar
                var mensaje = await _mensajesService.ObtenerPorIdAsync(id);
                if (mensaje == null)
                    return NotFound(new { mensaje = "Mensaje no encontrado" });

                // Solo puede eliminar sus propios mensajes
                if (mensaje.UsuarioId.HasValue && mensaje.UsuarioId != userId.Value)
                    return Forbid();

                await _mensajesService.EliminarAsync(id);
                return Ok(new { mensaje = "Mensaje eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar mensaje", error = ex.Message });
            }
        }
    }
}
