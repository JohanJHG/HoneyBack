using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            try
            {
                var mensajes = await _mensajesService.ObtenerTodosAsync();
                return Ok(mensajes);
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
                var mensaje = await _mensajesService.ObtenerPorIdAsync(id);
                if (mensaje == null)
                    return NotFound(new { mensaje = "Mensaje no encontrado" });

                return Ok(mensaje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener mensaje", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MensajesContacto>> Crear([FromBody] MensajesContacto mensaje)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
                var resultado = await _mensajesService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Mensaje no encontrado" });

                return Ok(new { mensaje = "Mensaje eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar mensaje", error = ex.Message });
            }
        }
    }
}
