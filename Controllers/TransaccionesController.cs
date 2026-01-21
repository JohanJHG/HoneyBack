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
    public class TransaccionesController : ControllerBase
    {
        private readonly ITransaccionesService _transaccionesService;

        public TransaccionesController(ITransaccionesService transaccionesService)
        {
            _transaccionesService = transaccionesService;
        }

        /// <summary>
        /// Obtiene todas las transacciones del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerMisTransacciones()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var transacciones = await _transaccionesService.ObtenerPorUsuarioAsync(userId.Value);
                var response = transacciones.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transacciones", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransaccionResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var transaccion = await _transaccionesService.ObtenerPorIdAsync(id);
                if (transaccion == null)
                    return NotFound(new { mensaje = "Transaccion no encontrada" });

                // Validacion de propiedad
                if (transaccion.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(MapToDto(transaccion));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transaccion", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS transacciones
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var transacciones = await _transaccionesService.ObtenerPorUsuarioAsync(usuarioId);
                var response = transacciones.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transacciones del usuario", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorTipo(int usuarioId, string tipo)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS transacciones
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                // Validar tipo
                if (tipo.ToLower() != "ingreso" && tipo.ToLower() != "gasto")
                    return BadRequest(new { mensaje = "Tipo debe ser 'ingreso' o 'gasto'" });

                var transacciones = await _transaccionesService.ObtenerPorUsuarioYTipoAsync(usuarioId, tipo);
                var response = transacciones.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transacciones por tipo", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/categoria/{categoria}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorCategoria(int usuarioId, string categoria)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS transacciones
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var transacciones = await _transaccionesService.ObtenerPorUsuarioYCategoriaAsync(usuarioId, categoria);
                var response = transacciones.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transacciones por categoria", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/periodo")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorPeriodo(
            int usuarioId, 
            [FromQuery] string fechaInicio, 
            [FromQuery] string fechaFin)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS transacciones
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                if (!DateOnly.TryParse(fechaInicio, out var inicio))
                    return BadRequest(new { mensaje = "fechaInicio invalida. Formato esperado: YYYY-MM-DD" });

                if (!DateOnly.TryParse(fechaFin, out var fin))
                    return BadRequest(new { mensaje = "fechaFin invalida. Formato esperado: YYYY-MM-DD" });

                var transacciones = await _transaccionesService.ObtenerPorUsuarioYFechaAsync(usuarioId, inicio, fin);
                var response = transacciones.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener transacciones por periodo", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TransaccionResponseDto>> Crear([FromBody] TransaccionCreateDto transaccionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar tipo
                if (transaccionDto.Tipo.ToLower() != "ingreso" && transaccionDto.Tipo.ToLower() != "gasto")
                    return BadRequest(new { mensaje = "Tipo debe ser 'ingreso' o 'gasto'" });

                var transaccion = new Transaccione
                {
                    // Usar userId DEL TOKEN, NO del DTO
                    UsuarioId = userId.Value,
                    Nombre = transaccionDto.Nombre,
                    Monto = transaccionDto.Monto,
                    Tipo = transaccionDto.Tipo,
                    Fecha = transaccionDto.Fecha,
                    Categoria = transaccionDto.Categoria
                };

                var transaccionCreada = await _transaccionesService.CrearAsync(transaccion);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = transaccionCreada.TransaccionId }, MapToDto(transaccionCreada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear transaccion", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransaccionResponseDto>> Actualizar(int id, [FromBody] TransaccionUpdateDto transaccionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var transaccionExistente = await _transaccionesService.ObtenerPorIdAsync(id);
                if (transaccionExistente == null)
                    return NotFound(new { mensaje = "Transaccion no encontrada" });

                if (transaccionExistente.UsuarioId != userId.Value)
                    return Forbid();

                // Validar tipo si se proporciona
                if (!string.IsNullOrEmpty(transaccionDto.Tipo) && 
                    transaccionDto.Tipo.ToLower() != "ingreso" && 
                    transaccionDto.Tipo.ToLower() != "gasto")
                {
                    return BadRequest(new { mensaje = "Tipo debe ser 'ingreso' o 'gasto'" });
                }

                var transaccion = new Transaccione
                {
                    Nombre = transaccionDto.Nombre ?? transaccionExistente.Nombre,
                    Monto = transaccionDto.Monto ?? transaccionExistente.Monto,
                    Tipo = transaccionDto.Tipo ?? transaccionExistente.Tipo,
                    Fecha = transaccionDto.Fecha ?? transaccionExistente.Fecha,
                    Categoria = transaccionDto.Categoria ?? transaccionExistente.Categoria
                };

                var transaccionActualizada = await _transaccionesService.ActualizarAsync(id, transaccion);
                return Ok(MapToDto(transaccionActualizada!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar transaccion", error = ex.Message });
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
                var transaccion = await _transaccionesService.ObtenerPorIdAsync(id);
                if (transaccion == null)
                    return NotFound(new { mensaje = "Transaccion no encontrada" });

                if (transaccion.UsuarioId != userId.Value)
                    return Forbid();

                await _transaccionesService.EliminarAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar transaccion", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene resumen de totales del mes actual para el usuario
        /// </summary>
        [HttpGet("usuario/{usuarioId}/resumen/{anio}/{mes}")]
        public async Task<ActionResult> ObtenerResumenMensual(int usuarioId, int anio, int mes)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS datos
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var totalIngresos = await _transaccionesService.ObtenerTotalIngresosPorUsuarioAsync(usuarioId, anio, mes);
                var totalGastos = await _transaccionesService.ObtenerTotalGastosPorUsuarioAsync(usuarioId, anio, mes);

                return Ok(new
                {
                    anio,
                    mes,
                    totalIngresos,
                    totalGastos,
                    balance = totalIngresos - totalGastos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener resumen mensual", error = ex.Message });
            }
        }

        private static TransaccionResponseDto MapToDto(Transaccione transaccion)
        {
            return new TransaccionResponseDto
            {
                TransaccionId = transaccion.TransaccionId,
                UsuarioId = transaccion.UsuarioId,
                Nombre = transaccion.Nombre,
                Monto = transaccion.Monto,
                Tipo = transaccion.Tipo,
                Fecha = transaccion.Fecha.ToString("yyyy-MM-dd"),
                Categoria = transaccion.Categoria,
                FechaCreacion = transaccion.FechaCreacion
            };
        }
    }
}
