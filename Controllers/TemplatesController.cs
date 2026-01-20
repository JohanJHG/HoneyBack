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
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplatesService _templatesService;

        public TemplatesController(ITemplatesService templatesService)
        {
            _templatesService = templatesService;
        }

        /// <summary>
        /// Obtiene todos los templates del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateResponseDto>>> ObtenerMisTemplates()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var templates = await _templatesService.ObtenerPorUsuarioAsync(userId.Value);
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
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var template = await _templatesService.ObtenerPorIdAsync(id);
                if (template == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                // Validación de propiedad
                if (template.UsuarioId != userId.Value)
                    return Forbid();

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
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS templates
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

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
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS templates
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

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
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS templates
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

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

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar nombre único para ESTE usuario
                if (await _templatesService.ExisteNombreAsync(templateDto.Nombre, userId.Value))
                {
                    return Conflict(new { mensaje = "Ya existe un template con ese nombre" });
                }

                var template = new Template
                {
                    // Usar userId DEL TOKEN, NO del DTO
                    UsuarioId = userId.Value,
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

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var templateExistente = await _templatesService.ObtenerPorIdAsync(id);
                if (templateExistente == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                if (templateExistente.UsuarioId != userId.Value)
                    return Forbid();

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
                return Ok(MapToDto(templateActualizado!));
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
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad
                var template = await _templatesService.ObtenerPorIdAsync(id);
                if (template == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                if (template.UsuarioId != userId.Value)
                    return Forbid();

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
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad
                var template = await _templatesService.ObtenerPorIdAsync(id);
                if (template == null)
                    return NotFound(new { mensaje = "Template no encontrado" });

                if (template.UsuarioId != userId.Value)
                    return Forbid();

                await _templatesService.EliminarAsync(id);
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
