using HoneyBack.Models;
using HoneyBack.Servicios;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Agregar autorización global
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;

        public ReportesController(IReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        /// <summary>
        /// Obtiene todos los reportes del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerMisReportes()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var reportes = await _reportesService.ObtenerPorUsuarioAsync(userId.Value);
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reporte>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var reporte = await _reportesService.ObtenerPorIdAsync(id);
                if (reporte == null)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                // Validación de propiedad
                if (reporte.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reporte", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS reportes
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var reportes = await _reportesService.ObtenerPorUsuarioAsync(usuarioId);
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpGet("estado/{estado}")]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerPorEstado(string estado)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Obtener solo reportes del usuario con ese estado
                var reportes = await _reportesService.ObtenerPorUsuarioAsync(userId.Value);
                var reportesFiltrados = reportes.Where(r => r.Estado == estado);
                return Ok(reportesFiltrados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Reporte>> Crear([FromBody] Reporte reporte)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Usar userId DEL TOKEN
                reporte.UsuarioId = userId.Value;

                var nuevoReporte = await _reportesService.CrearAsync(reporte);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoReporte.ReporteId }, nuevoReporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear reporte", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Reporte>> Actualizar(int id, [FromBody] Reporte reporte)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var reporteExistente = await _reportesService.ObtenerPorIdAsync(id);
                if (reporteExistente == null)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                if (reporteExistente.UsuarioId != userId.Value)
                    return Forbid();

                var reporteActualizado = await _reportesService.ActualizarAsync(id, reporte);
                return Ok(reporteActualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar reporte", error = ex.Message });
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
                var reporte = await _reportesService.ObtenerPorIdAsync(id);
                if (reporte == null)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                if (reporte.UsuarioId != userId.Value)
                    return Forbid();

                await _reportesService.EliminarAsync(id);
                return Ok(new { mensaje = "Reporte eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar reporte", error = ex.Message });
            }
        }
    }
}
