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
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplatesService _templatesService;

        public TemplatesController(ITemplatesService templatesService)
        {
            _templatesService = templatesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateResponseDto>>> ObtenerTodos()
        {
            try
            {
                var templates = await _templatesService.ObtenerTodosAsync();
                var response = templates.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener templates", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var template = await _templatesService.ObtenerPorIdAsync(id);
                if (template == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                return Ok(MapToDto(template));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener template", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<TemplateResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var templates = await _templatesService.ObtenerPorUsuarioAsync(usuarioId);
                var response = templates.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener templates del usuario", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/activos")]
        public async Task<ActionResult<IEnumerable<TemplateResponseDto>>> ObtenerActivosPorUsuario(int usuarioId)
        {
            try
            {
                var templates = await _templatesService.ObtenerActivosPorUsuarioAsync(usuarioId);
                var response = templates.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener templates activos", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/mas-usados")]
        public async Task<ActionResult<IEnumerable<TemplateResponseDto>>> ObtenerMasUsados(int usuarioId, [FromQuery] int cantidad = 10)
        {
            try
            {
                var templates = await _templatesService.ObtenerMasUsadosAsync(usuarioId, cantidad);
                var response = templates.Select(t => MapToDto(t));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener templates más usados", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TemplateResponseDto>> Crear([FromBody] TemplateCreateDto templateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validar si ya existe
                if (await _templatesService.ExisteNombreAsync(templateDto.Nombre, templateDto.UsuarioId))
                {
                    return Conflict(new { mensaje = "Ya existe un template con ese nombre para el usuario" });
                }

                var template = new Template
                {
                    UsuarioId = templateDto.UsuarioId,
                    Nombre = templateDto.Nombre,
                    CategoriaId = templateDto.CategoriaId,
                    Monto = templateDto.Monto,
                    Descripcion = templateDto.Descripcion,
                    Tipo = templateDto.Tipo
                };

                var templateCreado = await _templatesService.CrearAsync(template);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = templateCreado.TemplateId }, MapToDto(templateCreado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear template", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TemplateResponseDto>> Actualizar(int id, [FromBody] TemplateUpdateDto templateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var template = new Template
                {
                    Nombre = templateDto.Nombre,
                    CategoriaId = templateDto.CategoriaId,
                    Monto = templateDto.Monto,
                    Descripcion = templateDto.Descripcion,
                    Tipo = templateDto.Tipo,
                    Activo = templateDto.Activo
                };

                var templateActualizado = await _templatesService.ActualizarAsync(id, template);
                if (templateActualizado == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                return Ok(MapToDto(templateActualizado));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar template", error = ex.Message });
            }
        }

        [HttpPost("{id}/usar")]
        public async Task<ActionResult> RegistrarUso(int id)
        {
            try
            {
                var resultado = await _templatesService.RegistrarUsoAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Template no encontrado" });

                return Ok(new { mensaje = "Uso de template registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar uso", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _templatesService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Template no encontrado" });

                return Ok(new { mensaje = "Template eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar template", error = ex.Message });
            }
        }

        private TemplateResponseDto MapToDto(Template template)
        {
            return new TemplateResponseDto
            {
                TemplateId = template.TemplateId,
                UsuarioId = template.UsuarioId,
                Nombre = template.Nombre,
                CategoriaId = template.CategoriaId,
                CategoriaNombre = template.Categoria?.Nombre,
                Monto = template.Monto,
                Descripcion = template.Descripcion,
                Tipo = template.Tipo,
                FrecuenciaUso = template.FrecuenciaUso ?? 0,
                FechaCreacion = template.FechaCreacion ?? DateTime.Now,
                FechaUltimoUso = template.FechaUltimoUso,
                Activo = template.Activo ?? true
            };
        }
    }
}
