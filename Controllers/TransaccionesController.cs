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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerMisTransacciones()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            var transacciones = await _transaccionesService.ObtenerPorUsuarioAsync(userId.Value);
            return Ok(transacciones.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransaccionResponseDto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            var transaccion = await _transaccionesService.ObtenerPorIdAsync(id);
            if (transaccion == null)
                return NotFound(new { message = "Transaccion no encontrada" });

            if (transaccion.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToDto(transaccion));
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var transacciones = await _transaccionesService.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(transacciones.Select(MapToDto));
        }

        [HttpGet("usuario/{usuarioId}/tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorTipo(int usuarioId, string tipo)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            if (tipo.ToLower() != "ingreso" && tipo.ToLower() != "gasto")
                return BadRequest(new { message = "Tipo debe ser 'ingreso' o 'gasto'" });

            var transacciones = await _transaccionesService.ObtenerPorUsuarioYTipoAsync(usuarioId, tipo);
            return Ok(transacciones.Select(MapToDto));
        }

        [HttpGet("usuario/{usuarioId}/categoria/{categoria}")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorCategoria(int usuarioId, string categoria)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            var transacciones = await _transaccionesService.ObtenerPorUsuarioYCategoriaAsync(usuarioId, categoria);
            return Ok(transacciones.Select(MapToDto));
        }

        [HttpGet("usuario/{usuarioId}/periodo")]
        public async Task<ActionResult<IEnumerable<TransaccionResponseDto>>> ObtenerPorPeriodo(
            int usuarioId,
            [FromQuery] string fechaInicio,
            [FromQuery] string fechaFin)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            if (!DateOnly.TryParse(fechaInicio, out var inicio))
                return BadRequest(new { message = "fechaInicio inválida. Formato esperado: YYYY-MM-DD" });

            if (!DateOnly.TryParse(fechaFin, out var fin))
                return BadRequest(new { message = "fechaFin inválida. Formato esperado: YYYY-MM-DD" });

            if (inicio > fin)
                return BadRequest(new { message = "fechaInicio no puede ser posterior a fechaFin" });

            var transacciones = await _transaccionesService.ObtenerPorUsuarioYFechaAsync(usuarioId, inicio, fin);
            return Ok(transacciones.Select(MapToDto));
        }

        [HttpPost]
        public async Task<ActionResult<TransaccionResponseDto>> Crear([FromBody] TransaccionCreateDto transaccionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (transaccionDto.Tipo.ToLower() != "ingreso" && transaccionDto.Tipo.ToLower() != "gasto")
                return BadRequest(new { message = "Tipo debe ser 'ingreso' o 'gasto'" });

            var transaccion = new Transaccione
            {
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

        [HttpPut("{id}")]
        public async Task<ActionResult<TransaccionResponseDto>> Actualizar(int id, [FromBody] TransaccionUpdateDto transaccionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            var transaccionExistente = await _transaccionesService.ObtenerPorIdAsync(id);
            if (transaccionExistente == null)
                return NotFound(new { message = "Transaccion no encontrada" });

            if (transaccionExistente.UsuarioId != userId.Value)
                return Forbid();

            if (!string.IsNullOrEmpty(transaccionDto.Tipo) &&
                transaccionDto.Tipo.ToLower() != "ingreso" &&
                transaccionDto.Tipo.ToLower() != "gasto")
            {
                return BadRequest(new { message = "Tipo debe ser 'ingreso' o 'gasto'" });
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            var transaccion = await _transaccionesService.ObtenerPorIdAsync(id);
            if (transaccion == null)
                return NotFound(new { message = "Transaccion no encontrada" });

            if (transaccion.UsuarioId != userId.Value)
                return Forbid();

            await _transaccionesService.EliminarAsync(id);
            return NoContent();
        }

        [HttpGet("usuario/{usuarioId}/resumen/{anio}/{mes}")]
        public async Task<ActionResult> ObtenerResumenMensual(int usuarioId, int anio, int mes)
        {
            var tokenUserId = User.GetUserId();
            if (!tokenUserId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            if (usuarioId != tokenUserId.Value)
                return Forbid();

            if (mes < 1 || mes > 12)
                return BadRequest(new { message = "El mes debe estar entre 1 y 12" });

            if (anio < 2000 || anio > 2100)
                return BadRequest(new { message = "El año debe estar entre 2000 y 2100" });

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

        private static TransaccionResponseDto MapToDto(Transaccione transaccion) => new()
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
