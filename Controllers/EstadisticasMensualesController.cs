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
    public class EstadisticasMensualesController : ControllerBase
    {
        private readonly IEstadisticasMensualesService _estadisticasService;

        public EstadisticasMensualesController(IEstadisticasMensualesService estadisticasService)
        {
            _estadisticasService = estadisticasService;
        }

        /// <summary>
        /// Obtiene todas las estadísticas del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadisticaMensualResponseDto>>> ObtenerMisEstadisticas()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var estadisticas = await _estadisticasService.ObtenerPorUsuarioAsync(userId.Value);
                var response = estadisticas.Select(e => MapToDto(e));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstadisticaMensualResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var estadistica = await _estadisticasService.ObtenerPorIdAsync(id);
                if (estadistica == null)
                    return NotFound(new { mensaje = "Estadística no encontrada" });

                // Validación de propiedad
                if (estadistica.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(MapToDto(estadistica));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadística", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<EstadisticaMensualResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS estadísticas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var estadisticas = await _estadisticasService.ObtenerPorUsuarioAsync(usuarioId);
                var response = estadisticas.Select(e => MapToDto(e));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas del usuario", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/periodo/{anio}/{mes}")]
        public async Task<ActionResult<EstadisticaMensualResponseDto>> ObtenerPorPeriodo(int usuarioId, int anio, int mes)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS estadísticas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var estadistica = await _estadisticasService.ObtenerPorPeriodoAsync(usuarioId, anio, mes);
                if (estadistica == null)
                    return NotFound(new { mensaje = "Estadística no encontrada para el período especificado" });

                return Ok(MapToDto(estadistica));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadística por período", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/anio/{anio}")]
        public async Task<ActionResult<IEnumerable<EstadisticaMensualResponseDto>>> ObtenerPorAnio(int usuarioId, int anio)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS estadísticas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var estadisticas = await _estadisticasService.ObtenerPorAnioAsync(usuarioId, anio);
                var response = estadisticas.Select(e => MapToDto(e));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas del año", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<EstadisticaMensualResponseDto>> Crear([FromBody] EstadisticaMensualCreateDto estadisticaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var estadistica = new EstadisticasMensuale
                {
                    // Usar userId DEL TOKEN
                    UsuarioId = userId.Value,
                    Anio = estadisticaDto.Anio,
                    Mes = estadisticaDto.Mes,
                    TotalIngresos = estadisticaDto.TotalIngresos ?? 0,
                    TotalGastos = estadisticaDto.TotalGastos ?? 0,
                    NumTransacciones = estadisticaDto.NumTransacciones ?? 0,
                    CategoriaMayorGastoId = estadisticaDto.CategoriaMayorGastoId,
                    MontoMayorGasto = estadisticaDto.MontoMayorGasto
                };

                var estadisticaCreada = await _estadisticasService.CrearAsync(estadistica);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = estadisticaCreada.EstadisticaId }, MapToDto(estadisticaCreada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear estadística", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EstadisticaMensualResponseDto>> Actualizar(int id, [FromBody] EstadisticaMensualUpdateDto estadisticaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var estadisticaExistente = await _estadisticasService.ObtenerPorIdAsync(id);
                if (estadisticaExistente == null)
                    return NotFound(new { mensaje = "Estadística no encontrada" });

                if (estadisticaExistente.UsuarioId != userId.Value)
                    return Forbid();

                var estadistica = new EstadisticasMensuale
                {
                    TotalIngresos = estadisticaDto.TotalIngresos,
                    TotalGastos = estadisticaDto.TotalGastos,
                    NumTransacciones = estadisticaDto.NumTransacciones,
                    CategoriaMayorGastoId = estadisticaDto.CategoriaMayorGastoId,
                    MontoMayorGasto = estadisticaDto.MontoMayorGasto
                };

                var estadisticaActualizada = await _estadisticasService.ActualizarAsync(id, estadistica);
                return Ok(MapToDto(estadisticaActualizada!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar estadística", error = ex.Message });
            }
        }

        [HttpPost("usuario/{usuarioId}/recalcular/{anio}/{mes}")]
        public async Task<ActionResult> RecalcularEstadistica(int usuarioId, int anio, int mes)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede recalcular SUS estadísticas
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var resultado = await _estadisticasService.RecalcularEstadisticaAsync(usuarioId, anio, mes);
                if (!resultado)
                    return NotFound(new { mensaje = "Estadística no encontrada para recalcular" });

                return Ok(new { mensaje = "Estadística recalculada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al recalcular estadística", error = ex.Message });
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

                // Validar propiedad antes de eliminar
                var estadistica = await _estadisticasService.ObtenerPorIdAsync(id);
                if (estadistica == null)
                    return NotFound(new { mensaje = "Estadística no encontrada" });

                if (estadistica.UsuarioId != userId.Value)
                    return Forbid();

                await _estadisticasService.EliminarAsync(id);
                return Ok(new { mensaje = "Estadística eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar estadística", error = ex.Message });
            }
        }

        private EstadisticaMensualResponseDto MapToDto(EstadisticasMensuale estadistica)
        {
            return new EstadisticaMensualResponseDto
            {
                EstadisticaId = estadistica.EstadisticaId,
                UsuarioId = estadistica.UsuarioId,
                Anio = estadistica.Anio,
                Mes = estadistica.Mes,
                TotalIngresos = estadistica.TotalIngresos ?? 0,
                TotalGastos = estadistica.TotalGastos ?? 0,
                Balance = estadistica.Balance ?? 0,
                NumTransacciones = estadistica.NumTransacciones ?? 0,
                CategoriaMayorGastoId = estadistica.CategoriaMayorGastoId,
                CategoriaMayorGastoNombre = estadistica.CategoriaMayorGasto?.Nombre,
                MontoMayorGasto = estadistica.MontoMayorGasto,
                FechaCalculo = estadistica.FechaCalculo ?? DateTime.Now
            };
        }
    }
}
