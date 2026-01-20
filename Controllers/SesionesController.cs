using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HoneyBack.DTOs;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SesionesController : ControllerBase
    {
        private readonly ISesionesService _sesionesService;

        public SesionesController(ISesionesService sesionesService)
        {
            _sesionesService = sesionesService;
        }

        /// <summary>
        /// Obtiene todas las sesiones del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sesione>>> ObtenerMisSesiones()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(userId.Value);
                return Ok(sesiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesiones", error = ex.Message });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Sesione>> ObtenerPorId(long id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var sesion = await _sesionesService.ObtenerPorIdAsync(id);
                if (sesion == null)
                    return NotFound(new { mensaje = "Sesión no encontrada" });

                // Validación de propiedad
                if (sesion.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(sesion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesión", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<ActionResult<IEnumerable<Sesione>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS sesiones
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(usuarioId);
                return Ok(sesiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener sesiones", error = ex.Message });
            }
        }

        // Acepta string plano o objeto con token/tokenSesion
        [HttpPost("validar")]
        [AllowAnonymous] // Permitir validación sin autenticación
        [Consumes("application/json")]
        public async Task<ActionResult> ValidarToken([FromBody] JsonElement payload)
        {
            try
            {
                string? token = null;

                if (payload.ValueKind == JsonValueKind.String)
                {
                    token = payload.GetString();
                }
                else if (payload.ValueKind == JsonValueKind.Object)
                {
                    if (payload.TryGetProperty("token", out var t)) token = t.GetString();
                    else if (payload.TryGetProperty("tokenSesion", out var ts)) token = ts.GetString();
                }

                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(new { mensaje = "Body inválido. Envíe un string JSON con el token o un objeto con 'token'/'tokenSesion'." });

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

        // Este endpoint NO debe ser público - solo AuthController debe crear sesiones
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)] // Ocultar de Swagger
        public async Task<ActionResult<Sesione>> Crear([FromBody] SesionCreateDto sesionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede crear sesiones para sí mismo
                if (sesionDto.UsuarioId != userId.Value)
                    return Forbid();

                var sesion = new Sesione
                {
                    UsuarioId = userId.Value,
                    TokenSesion = sesionDto.TokenSesion,
                    FechaExpiracion = sesionDto.FechaExpiracion
                };

                var nuevaSesion = await _sesionesService.CrearAsync(sesion);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaSesion.SesionId }, nuevaSesion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear sesión", error = ex.Message });
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult> Eliminar(long id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de eliminar
                var sesion = await _sesionesService.ObtenerPorIdAsync(id);
                if (sesion == null)
                    return NotFound(new { mensaje = "Sesión no encontrada" });

                if (sesion.UsuarioId != userId.Value)
                    return Forbid();

                await _sesionesService.EliminarAsync(id);
                return Ok(new { mensaje = "Sesión eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar sesión", error = ex.Message });
            }
        }

        [HttpPost("limpiar-expiradas")]
        [AllowAnonymous] // Puede ser llamado por un job o manualmente
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
