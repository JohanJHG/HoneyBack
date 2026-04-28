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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerMisMetas()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var metas = await _metasService.ObtenerPorUsuarioAsync(userId.Value);
            return Ok(metas.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MetaAhorroResponseDto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var meta = await _metasService.ObtenerPorIdAsync(id);
            if (meta == null)
                return NotFound(new { mensaje = "Meta no encontrada" });

            if (meta.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToDto(meta));
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var metas = await _metasService.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(metas.Select(MapToDto));
        }

        [HttpGet("usuario/{usuarioId}/activas")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerActivasPorUsuario(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var metas = await _metasService.ObtenerActivasPorUsuarioAsync(usuarioId);
            return Ok(metas.Select(MapToDto));
        }

        [HttpGet("usuario/{usuarioId}/completadas")]
        public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerCompletadasPorUsuario(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var metas = await _metasService.ObtenerCompletadasPorUsuarioAsync(usuarioId);
            return Ok(metas.Select(MapToDto));
        }

        [HttpPost]
        public async Task<ActionResult<MetaAhorroResponseDto>> Crear([FromBody] MetaAhorroCreateDto metaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var meta = new MetasAhorro
            {
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

        [HttpPut("{id}")]
        public async Task<ActionResult<MetaAhorroResponseDto>> Actualizar(int id, [FromBody] MetaAhorroUpdateDto metaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

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

        [HttpPatch("{id}/monto")]
        public async Task<ActionResult<MetaAhorroResponseDto>> ActualizarMonto(int id, [FromBody] decimal nuevoMonto)
        {
            if (nuevoMonto < 0)
                return BadRequest(new { mensaje = "El monto no puede ser negativo" });

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var metaExistente = await _metasService.ObtenerPorIdAsync(id);
            if (metaExistente == null)
                return NotFound(new { mensaje = "Meta no encontrada" });

            if (metaExistente.UsuarioId != userId.Value)
                return Forbid();

            var metaActualizada = await _metasService.ActualizarMontoAsync(id, nuevoMonto);
            return Ok(MapToDto(metaActualizada!));
        }

        [HttpPost("{id}/completar")]
        public async Task<ActionResult> MarcarComoCompletada(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var meta = await _metasService.ObtenerPorIdAsync(id);
            if (meta == null)
                return NotFound(new { mensaje = "Meta no encontrada" });

            if (meta.UsuarioId != userId.Value)
                return Forbid();

            await _metasService.EliminarAsync(id);
            return Ok(new { mensaje = "Meta eliminada exitosamente" });
        }

        private static MetaAhorroResponseDto MapToDto(MetasAhorro meta) => new()
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
