using HoneyBack.DTOs;
using HoneyBack.Extensions;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerTodos()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(userId.Value);
            return Ok(sesiones.Select(MapToResponse));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<object>> ObtenerPorId(long id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var sesion = await _sesionesService.ObtenerPorIdAsync(id);
            if (sesion == null)
                return NotFound(new { mensaje = "Sesion no encontrada" });

            if (sesion.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToResponse(sesion));
        }

        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerPorUsuario(int usuarioId)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != userId.Value)
                return Forbid();

            var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(sesiones.Select(MapToResponse));
        }

        [HttpPost]
        public async Task<ActionResult<object>> Crear([FromBody] SesionCreateDto sesionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (sesionDto.UsuarioId != userId.Value)
                return Forbid();

            var sesion = new Sesione
            {
                UsuarioId = userId.Value,
                TokenSesion = sesionDto.TokenSesion,
                FechaExpiracion = DateTime.SpecifyKind(sesionDto.FechaExpiracion, DateTimeKind.Unspecified),
                FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var creada = await _sesionesService.CrearAsync(sesion);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.SesionId }, MapToResponse(creada));
        }

        [HttpPost("validar")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidarToken([FromBody] JsonElement payload)
        {
            string? token = null;

            if (payload.ValueKind == JsonValueKind.String)
            {
                token = payload.GetString();
            }
            else if (payload.ValueKind == JsonValueKind.Object)
            {
                if (payload.TryGetProperty("token", out var tokenProp))
                    token = tokenProp.GetString();
                else if (payload.TryGetProperty("tokenSesion", out var tokenSesionProp))
                    token = tokenSesionProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { mensaje = "Token no proporcionado" });

            var esValido = await _sesionesService.ValidarTokenAsync(token);
            return Ok(new
            {
                valido = esValido,
                mensaje = esValido ? "Token válido" : "Token inválido o expirado"
            });
        }

        [HttpPost("limpiar-expiradas")]
        public async Task<ActionResult> LimpiarSesionesExpiradas()
        {
            await _sesionesService.LimpiarSesionesExpiradasAsync();
            return Ok(new { mensaje = "Sesiones expiradas limpiadas exitosamente" });
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult> Eliminar(long id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var sesion = await _sesionesService.ObtenerPorIdAsync(id);
            if (sesion == null)
                return NotFound(new { mensaje = "Sesion no encontrada" });

            if (sesion.UsuarioId != userId.Value)
                return Forbid();

            var eliminado = await _sesionesService.EliminarAsync(id);
            if (!eliminado)
                return NotFound(new { mensaje = "Sesion no encontrada" });

            return Ok(new { mensaje = "Sesion eliminada exitosamente" });
        }

        private static object MapToResponse(Sesione sesion)
        {
            return new
            {
                sesion.SesionId,
                sesion.UsuarioId,
                sesion.TokenSesion,
                sesion.FechaExpiracion,
                sesion.FechaCreacion,
                sesion.Ipaddress,
                sesion.UserAgent,
                sesion.Activa
            };
        }
    }
}
