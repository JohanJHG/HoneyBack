using HoneyBack.DTOs;
using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MensajesContactoController : ControllerBase
    {
        private readonly IMensajesContactoService _mensajesService;

        public MensajesContactoController(IMensajesContactoService mensajesService)
        {
            _mensajesService = mensajesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MensajesContacto>>> ObtenerTodos()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var mensajes = await _mensajesService.ObtenerTodosAsync();
            var mensajesUsuario = mensajes.Where(m => m.UsuarioId == userId.Value);
            return Ok(mensajesUsuario);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MensajesContacto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var mensaje = await _mensajesService.ObtenerPorIdAsync(id);
            if (mensaje == null)
                return NotFound(new { mensaje = "Mensaje no encontrado" });

            if (mensaje.UsuarioId.HasValue && mensaje.UsuarioId != userId.Value)
                return Forbid();

            return Ok(mensaje);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<MensajesContacto>> Crear([FromBody] MensajeContactoCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault() ?? "Datos inválidos" });

            var mensaje = new MensajesContacto
            {
                Nombre = request.Nombre.Trim(),
                Email = request.Email.Trim().ToLower(),
                Mensaje = request.Mensaje.Trim(),
                FechaEnvio = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var userId = User.GetUserId();
            if (userId.HasValue)
                mensaje.UsuarioId = userId.Value;

            var nuevoMensaje = await _mensajesService.CrearAsync(mensaje);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoMensaje.ContactoId }, new { nuevoMensaje.ContactoId, nuevoMensaje.Nombre, nuevoMensaje.Email, nuevoMensaje.FechaEnvio });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var mensaje = await _mensajesService.ObtenerPorIdAsync(id);
            if (mensaje == null)
                return NotFound(new { mensaje = "Mensaje no encontrado" });

            if (mensaje.UsuarioId.HasValue && mensaje.UsuarioId != userId.Value)
                return Forbid();

            await _mensajesService.EliminarAsync(id);
            return Ok(new { mensaje = "Mensaje eliminado exitosamente" });
        }
    }
}
