using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SesionesController : ControllerBase
    {
        private readonly ISesionesService _sesionesService;

        public SesionesController(ISesionesService sesionesService)
        {
            _sesionesService = sesionesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sesione>>> ObtenerTodos()
        {
            try
            {
                var sesiones = await _sesionesService.ObtenerTodosAsync();
                return Ok(sesiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesiones", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sesione>> ObtenerPorId(long id)
        {
            try
            {
                var sesion = await _sesionesService.ObtenerPorIdAsync(id);
                if (sesion == null)
                    return NotFound(new { mensaje = "Sesión no encontrada" });

                return Ok(sesion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesión", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Sesione>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(usuarioId);
                return Ok(sesiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesiones", error = ex.Message });
            }
        }

        [HttpPost("validar")]
        public async Task<ActionResult> ValidarToken([FromBody] string token)
        {
            try
            {
                var esValido = await _sesionesService.ValidarTokenAsync(token);
                if (!esValido)
                    return Unauthorized(new { mensaje = "Token inválido o expirado" });

                return Ok(new { mensaje = "Token válido" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al validar token", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Sesione>> Crear([FromBody] Sesione sesion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var nuevaSesion = await _sesionesService.CrearAsync(sesion);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaSesion.SesionId }, nuevaSesion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear sesión", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(long id)
        {
            try
            {
                var resultado = await _sesionesService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Sesión no encontrada" });

                return Ok(new { mensaje = "Sesión eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar sesión", error = ex.Message });
            }
        }

        [HttpPost("limpiar-expiradas")]
        public async Task<ActionResult> LimpiarSesionesExpiradas()
        {
            try
            {
                await _sesionesService.LimpiarSesionesExpiradasAsync();
                return Ok(new { mensaje = "Sesiones expiradas eliminadas exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al limpiar sesiones", error = ex.Message });
            }
        }
    }
}
