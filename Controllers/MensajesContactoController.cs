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
        private readonly IEmailService _emailService;
        private readonly ILogger<MensajesContactoController> _logger;

        public MensajesContactoController(
            IMensajesContactoService mensajesService,
            IEmailService emailService,
            ILogger<MensajesContactoController> logger)
        {
            _mensajesService = mensajesService;
            _emailService = emailService;
            _logger = logger;
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
                return BadRequest(new { mensaje = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault() ?? "Datos inválidos" });

            var nombre = request.Nombre.Trim();
            var email = request.Email.Trim().ToLower();
            var mensajeTexto = request.Mensaje.Trim();

            var entidad = new MensajesContacto
            {
                Nombre = nombre,
                Email = email,
                Mensaje = mensajeTexto,
                FechaEnvio = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var userId = User.GetUserId();
            if (userId.HasValue)
                entidad.UsuarioId = userId.Value;

            var nuevoMensaje = await _mensajesService.CrearAsync(entidad);
            _logger.LogInformation(
                "Nuevo mensaje de contacto guardado. Id={ContactoId} De={Email} UsuarioId={UsuarioId}",
                nuevoMensaje.ContactoId, email, userId?.ToString() ?? "anónimo");

            // Notificar a soporte sin bloquear la respuesta al cliente
            _ = _emailService.SendContactNotificationAsync(nombre, email, mensajeTexto)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogError(t.Exception, "Error al enviar notificación de contacto para Id={ContactoId}", nuevoMensaje.ContactoId);
                });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoMensaje.ContactoId },
                new { nuevoMensaje.ContactoId, nuevoMensaje.Nombre, nuevoMensaje.Email, nuevoMensaje.FechaEnvio });
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
