using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.DTOs;
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
        public async Task<ActionResult<IEnumerable<ConfiguracionUsuarioResponseDto>>> ObtenerTodos()
        {
            try
            {
                var configuraciones = await _configuracionesService.ObtenerTodosAsync();
                var response = configuraciones.Select(c => MapToDto(c));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener configuraciones", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var configuracion = await _configuracionesService.ObtenerPorIdAsync(id);
                if (configuracion == null)
                    return NotFound(new { mensaje = "Configuración no encontrada" });

                return Ok(MapToDto(configuracion));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener configuración", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> ObtenerPorUsuarioId(int usuarioId)
        {
            try
            {
                var configuracion = await _configuracionesService.ObtenerPorUsuarioIdAsync(usuarioId);
                if (configuracion == null)
                    return NotFound(new { mensaje = "Configuración no encontrada para el usuario" });

                return Ok(MapToDto(configuracion));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener configuración del usuario", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> Crear([FromBody] ConfiguracionUsuarioCreateDto configuracionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Verificar si ya existe configuración para el usuario
                var existente = await _configuracionesService.ObtenerPorUsuarioIdAsync(configuracionDto.UsuarioId);
                if (existente != null)
                {
                    return Conflict(new { mensaje = "Ya existe una configuración para este usuario" });
                }

                var configuracion = new ConfiguracionesUsuario
                {
                    UsuarioId = configuracionDto.UsuarioId,
                    NotificacionesEmail = configuracionDto.NotificacionesEmail,
                    NotificacionesPush = configuracionDto.NotificacionesPush,
                    Tema = configuracionDto.Tema,
                    Idioma = configuracionDto.Idioma,
                    Timezone = configuracionDto.Timezone,
                    FormatoFecha = configuracionDto.FormatoFecha,
                    PrimeraVez = configuracionDto.PrimeraVez
                };

                var configuracionCreada = await _configuracionesService.CrearAsync(configuracion);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = configuracionCreada.ConfiguracionId }, MapToDto(configuracionCreada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear configuración", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ConfiguracionUsuarioResponseDto>> Actualizar(int id, [FromBody] ConfiguracionUsuarioUpdateDto configuracionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var configuracion = new ConfiguracionesUsuario
                {
                    NotificacionesEmail = configuracionDto.NotificacionesEmail,
                    NotificacionesPush = configuracionDto.NotificacionesPush,
                    Tema = configuracionDto.Tema,
                    Idioma = configuracionDto.Idioma,
                    Timezone = configuracionDto.Timezone,
                    FormatoFecha = configuracionDto.FormatoFecha,
                    PrimeraVez = configuracionDto.PrimeraVez,
                    ConfiguracionPersonalizada = configuracionDto.ConfiguracionPersonalizada
                };

                var configuracionActualizada = await _configuracionesService.ActualizarAsync(id, configuracion);
                if (configuracionActualizada == null)
                    return NotFound(new { mensaje = "Configuración no encontrada" });

                return Ok(MapToDto(configuracionActualizada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar configuración", error = ex.Message });
            }
        }

        [HttpPost("usuario/{usuarioId}/marcar-veterano")]
        public async Task<ActionResult> MarcarComoVeterano(int usuarioId)
        {
            try
            {
                var resultado = await _configuracionesService.MarcarComoVeterano(usuarioId);
                if (!resultado)
                    return NotFound(new { mensaje = "Configuración no encontrada" });

                return Ok(new { mensaje = "Usuario marcado como veterano exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al marcar como veterano", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _configuracionesService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Configuración no encontrada" });

                return Ok(new { mensaje = "Configuración eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar configuración", error = ex.Message });
            }
        }

        private ConfiguracionUsuarioResponseDto MapToDto(ConfiguracionesUsuario configuracion)
        {
            return new ConfiguracionUsuarioResponseDto
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
                FechaActualizacion = configuracion.FechaActualizacion ?? DateTime.Now
            };
        }
    }
}
