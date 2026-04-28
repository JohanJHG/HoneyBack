using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.DTOs;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfiguracionesUsuarioController : ControllerBase
    {
        private readonly IConfiguracionesUsuarioService _configuracionesService;

        public ConfiguracionesUsuarioController(IConfiguracionesUsuarioService configuracionesService)
        {
            _configuracionesService = configuracionesService;
        }

        [HttpGet]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> ObtenerMiConfiguracion()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var configuracion = await _configuracionesService.ObtenerPorUsuarioIdAsync(userId.Value);
            if (configuracion == null)
                return NotFound(new { mensaje = "Configuracion no encontrada" });

            return Ok(MapToDto(configuracion));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var configuracion = await _configuracionesService.ObtenerPorIdAsync(id);
            if (configuracion == null)
                return NotFound(new { mensaje = "Configuracion no encontrada" });

            if (configuracion.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToDto(configuracion));
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> ObtenerPorUsuarioId(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var configuracion = await _configuracionesService.ObtenerPorUsuarioIdAsync(usuarioId);
            if (configuracion == null)
                return NotFound(new { mensaje = "Configuracion no encontrada para el usuario" });

            return Ok(MapToDto(configuracion));
        }

        [HttpPost]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> Crear([FromBody] ConfiguracionUsuarioCreateDto configuracionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var existente = await _configuracionesService.ObtenerPorUsuarioIdAsync(userId.Value);
            if (existente != null)
                return Conflict(new { mensaje = "Ya existe una configuracion para este usuario" });

            var configuracion = new ConfiguracionesUsuario
            {
                UsuarioId = userId.Value,
                NotificacionesEmail = configuracionDto.NotificacionesEmail,
                NotificacionesPush = configuracionDto.NotificacionesPush,
                Tema = configuracionDto.Tema,
                Idioma = configuracionDto.Idioma,
                Timezone = configuracionDto.Timezone,
                FormatoFecha = configuracionDto.FormatoFecha,
                PrimeraVez = configuracionDto.PrimeraVez,
                MonedaPreferida = configuracionDto.MonedaPreferida,
                NombreUsuario = configuracionDto.NombreUsuario,
                AvatarUrl = configuracionDto.AvatarUrl
            };

            var configuracionCreada = await _configuracionesService.CrearAsync(configuracion);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = configuracionCreada.ConfiguracionId }, MapToDto(configuracionCreada));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> Actualizar(int id, [FromBody] ConfiguracionUsuarioUpdateDto configuracionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var configuracionExistente = await _configuracionesService.ObtenerPorIdAsync(id);
            if (configuracionExistente == null)
                return NotFound(new { mensaje = "Configuracion no encontrada" });

            if (configuracionExistente.UsuarioId != userId.Value)
                return Forbid();

            var configuracion = new ConfiguracionesUsuario
            {
                NotificacionesEmail = configuracionDto.NotificacionesEmail,
                NotificacionesPush = configuracionDto.NotificacionesPush,
                Tema = configuracionDto.Tema,
                Idioma = configuracionDto.Idioma,
                Timezone = configuracionDto.Timezone,
                FormatoFecha = configuracionDto.FormatoFecha,
                PrimeraVez = configuracionDto.PrimeraVez,
                ConfiguracionPersonalizada = configuracionDto.ConfiguracionPersonalizada,
                MonedaPreferida = configuracionDto.MonedaPreferida,
                NombreUsuario = configuracionDto.NombreUsuario,
                AvatarUrl = configuracionDto.AvatarUrl
            };

            var configuracionActualizada = await _configuracionesService.ActualizarAsync(id, configuracion);
            return Ok(MapToDto(configuracionActualizada!));
        }

        [HttpPost("usuario/{usuarioId}/marcar-veterano")]
        public async Task<ActionResult> MarcarComoVeterano(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var resultado = await _configuracionesService.MarcarComoVeterano(usuarioId);
            if (!resultado)
                return NotFound(new { mensaje = "Configuracion no encontrada" });

            return Ok(new { mensaje = "Usuario marcado como veterano exitosamente" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var configuracion = await _configuracionesService.ObtenerPorIdAsync(id);
            if (configuracion == null)
                return NotFound(new { mensaje = "Configuracion no encontrada" });

            if (configuracion.UsuarioId != userId.Value)
                return Forbid();

            await _configuracionesService.EliminarAsync(id);
            return Ok(new { mensaje = "Configuracion eliminada exitosamente" });
        }

        private ConfiguracionUsuarioResponseDto MapToDto(ConfiguracionesUsuario configuracion) => new()
        {
            ConfiguracionId = configuracion.ConfiguracionId,
            UsuarioId = configuracion.UsuarioId,
            NotificacionesEmail = configuracion.NotificacionesEmail ?? true,
            NotificacionesPush = configuracion.NotificacionesPush ?? true,
            Tema = configuracion.Tema ?? "dark",
            Idioma = configuracion.Idioma ?? "es",
            Timezone = configuracion.Timezone ?? "America/Bogota",
            FormatoFecha = configuracion.FormatoFecha ?? "DD/MM/YYYY",
            PrimeraVez = configuracion.PrimeraVez ?? true,
            ConfiguracionPersonalizada = configuracion.ConfiguracionPersonalizada,
            FechaActualizacion = configuracion.FechaActualizacion ?? DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            FechaCreacion = configuracion.FechaCreacion,
            MonedaPreferida = configuracion.MonedaPreferida ?? "COP",
            NombreUsuario = configuracion.NombreUsuario,
            AvatarUrl = configuracion.AvatarUrl,
            EsVeterano = configuracion.EsVeterano ?? false
        };
    }
}
