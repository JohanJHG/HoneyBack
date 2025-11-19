using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;

        public ReportesController(IReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerTodos()
        {
            try
            {
                var reportes = await _reportesService.ObtenerTodosAsync();
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Reporte>> ObtenerPorId(int id)
        {
            try
            {
                var reporte = await _reportesService.ObtenerPorIdAsync(id);
                if (reporte == null)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reporte", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var reportes = await _reportesService.ObtenerPorUsuarioAsync(usuarioId);
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpGet("estado/{estado}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Reporte>>> ObtenerPorEstado(string estado)
        {
            try
            {
                var reportes = await _reportesService.ObtenerPorEstadoAsync(estado);
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener reportes", error = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Reporte>> Crear([FromBody] Reporte reporte)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var nuevoReporte = await _reportesService.CrearAsync(reporte);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoReporte.ReporteId }, nuevoReporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear reporte", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Reporte>> Actualizar(int id, [FromBody] Reporte reporte)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reporteActualizado = await _reportesService.ActualizarAsync(id, reporte);
                if (reporteActualizado == null)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                return Ok(reporteActualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar reporte", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _reportesService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Reporte no encontrado" });

                return Ok(new { mensaje = "Reporte eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar reporte", error = ex.Message });
            }
        }
    }
}
