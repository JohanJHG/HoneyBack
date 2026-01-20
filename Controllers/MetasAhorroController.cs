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
    public class MetasAhorroController : ControllerBase
    {
        private readonly IMetasAhorroService _metasService;

        public MetasAhorroController(IMetasAhorroService metasService)
        {
            _metasService = metasService;
        }

        /// <summary>
        /// Obtiene todas las metas del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerMisMetas()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var metas = await _metasService.ObtenerPorUsuarioAsync(userId.Value);
                var response = metas.Select(m => MapToDto(m));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener metas", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MetaAhorroResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var meta = await _metasService.ObtenerPorIdAsync(id);
                if (meta == null)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                // Validación de propiedad
                if (meta.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(MapToDto(meta));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener meta", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS metas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var metas = await _metasService.ObtenerPorUsuarioAsync(usuarioId);
                var response = metas.Select(m => MapToDto(m));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener metas del usuario", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/activas")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerActivasPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS metas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var metas = await _metasService.ObtenerActivasPorUsuarioAsync(usuarioId);
                var response = metas.Select(m => MapToDto(m));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener metas activas", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/completadas")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerCompletadasPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS metas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var metas = await _metasService.ObtenerCompletadasPorUsuarioAsync(usuarioId);
                var response = metas.Select(m => MapToDto(m));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener metas completadas", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MetaAhorroResponseDto>> Crear([FromBody] MetaAhorroCreateDto metaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var meta = new MetasAhorro  
                {
                    // Usar userId DEL TOKEN, NO del DTO
                    UsuarioId = userId.Value,
                    Nombre = metaDto.Nombre,
                    Descripcion = metaDto.Descripcion,
                    Categoria = metaDto.Categoria ?? "otro",
                    MontoObjetivo = metaDto.MontoObjetivo,
                    MontoActual = metaDto.MontoActual ?? 0,
                    FechaObjetivo = metaDto.FechaObjetivo,
                    Color = metaDto.Color ?? "#FFD8A9",
                    Icono = metaDto.Icono,
                    Prioridad = metaDto.Prioridad ?? 0
                };

                var metaCreada = await _metasService.CrearAsync(meta);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = metaCreada.MetaId }, MapToDto(metaCreada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear meta", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MetaAhorroResponseDto>> Actualizar(int id, [FromBody] MetaAhorroUpdateDto metaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var metaExistente = await _metasService.ObtenerPorIdAsync(id);
                if (metaExistente == null)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                if (metaExistente.UsuarioId != userId.Value)
                    return Forbid();

                var meta = new MetasAhorro
                {
                    Nombre = metaDto.Nombre,
                    Descripcion = metaDto.Descripcion,
                    Categoria = metaDto.Categoria ?? "otro",
                    MontoObjetivo = metaDto.MontoObjetivo,
                    MontoActual = metaDto.MontoActual,
                    FechaObjetivo = metaDto.FechaObjetivo,
                    Color = metaDto.Color,
                    Icono = metaDto.Icono,
                    Prioridad = metaDto.Prioridad,
                    Activa = metaDto.Activa,
                    Completada = metaDto.Completada
                };

                var metaActualizada = await _metasService.ActualizarAsync(id, meta);
                return Ok(MapToDto(metaActualizada!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar meta", error = ex.Message });
            }
        }

        [HttpPatch("{id}/monto")]
        public async Task<ActionResult<MetaAhorroResponseDto>> ActualizarMonto(int id, [FromBody] decimal nuevoMonto)
        {
            try
            {
                if (nuevoMonto < 0)
                    return BadRequest(new { mensaje = "El monto no puede ser negativo" });

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad
                var metaExistente = await _metasService.ObtenerPorIdAsync(id);
                if (metaExistente == null)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                if (metaExistente.UsuarioId != userId.Value)
                    return Forbid();

                var metaActualizada = await _metasService.ActualizarMontoAsync(id, nuevoMonto);
                return Ok(MapToDto(metaActualizada!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar monto", error = ex.Message });
            }
        }

        [HttpPost("{id}/completar")]
        public async Task<ActionResult> MarcarComoCompletada(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad
                var meta = await _metasService.ObtenerPorIdAsync(id);
                if (meta == null)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                if (meta.UsuarioId != userId.Value)
                    return Forbid();

                var resultado = await _metasService.MarcarComoCompletadaAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                return Ok(new { mensaje = "Meta marcada como completada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al marcar meta como completada", error = ex.Message });
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

                // Validar propiedad
                var meta = await _metasService.ObtenerPorIdAsync(id);
                if (meta == null)
                    return NotFound(new { mensaje = "Meta no encontrada" });

                if (meta.UsuarioId != userId.Value)
                    return Forbid();

                await _metasService.EliminarAsync(id);
                return Ok(new { mensaje = "Meta eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar meta", error = ex.Message });
            }
        }

        private MetaAhorroResponseDto MapToDto(MetasAhorro meta)
        {
            return new MetaAhorroResponseDto
            {
                MetaId = meta.MetaId,
                UsuarioId = meta.UsuarioId,
                Nombre = meta.Nombre,
                Descripcion = meta.Descripcion,
                Categoria = meta.Categoria ?? "otro",
                MontoObjetivo = meta.MontoObjetivo,
                MontoActual = meta.MontoActual ?? 0,
                FechaInicio = meta.FechaInicio,
                FechaObjetivo = meta.FechaObjetivo,
                FechaCompletada = meta.FechaCompletada,
                Color = meta.Color,
                Icono = meta.Icono,
                Prioridad = meta.Prioridad,
                Activa = meta.Activa,
                Completada = meta.Completada
            };
        }
    }
}
