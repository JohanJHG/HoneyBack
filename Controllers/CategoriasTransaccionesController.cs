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
    public class CategoriasTransaccionesController : ControllerBase
    {
        private readonly ICategoriasTransaccionesService _categoriasService;

        public CategoriasTransaccionesController(ICategoriasTransaccionesService categoriasService)
        {
            _categoriasService = categoriasService;
        }

        /// <summary>
        /// Obtiene las categorías del usuario autenticado (incluye las de sistema)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerMisCategorias()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Retorna categorías del sistema + las del usuario
                var categorias = await _categoriasService.ObtenerPorUsuarioAsync(userId.Value);
                var response = categorias.Select(c => MapToDto(c));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categorías", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaTransaccionResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var categoria = await _categoriasService.ObtenerPorIdAsync(id);
                if (categoria == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                // Permitir acceso si es categoría del sistema o pertenece al usuario
                if (categoria.EsSistema != true && categoria.UsuarioId != userId.Value)
                    return Forbid();

                return Ok(MapToDto(categoria));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categoría", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS categorías
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var categorias = await _categoriasService.ObtenerPorUsuarioAsync(usuarioId);
                var response = categorias.Select(c => MapToDto(c));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categorías del usuario", error = ex.Message });
            }
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerPorTipo(string tipo)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Filtrar por tipo y usuario
                var categorias = await _categoriasService.ObtenerPorTipoYUsuarioAsync(tipo, userId.Value);
                var response = categorias.Select(c => MapToDto(c));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categorías por tipo", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}/activas")]
        public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerActivasPorUsuario(int usuarioId)
        {
            try
            {
                var tokenUserId = User.GetUserId();
                if (!tokenUserId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Solo puede consultar SUS categorías
                if (usuarioId != tokenUserId.Value)
                    return Forbid();

                var categorias = await _categoriasService.ObtenerCategoriasActivasAsync(usuarioId);
                var response = categorias.Select(c => MapToDto(c));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categorías activas", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaTransaccionResponseDto>> Crear([FromBody] CategoriaTransaccionCreateDto categoriaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar si ya existe para este usuario
                if (await _categoriasService.ExisteNombreAsync(categoriaDto.Nombre, userId.Value, categoriaDto.Tipo))
                {
                    return Conflict(new { mensaje = "Ya existe una categoría con ese nombre para el usuario" });
                }

                var categoria = new CategoriasTransaccione
                {
                    Nombre = categoriaDto.Nombre,
                    Tipo = categoriaDto.Tipo,
                    Color = categoriaDto.Color ?? "#FFD8A9",
                    Icono = categoriaDto.Icono,
                    EsSistema = false, // Las categorías creadas por usuarios nunca son de sistema
                    UsuarioId = userId.Value, // Usar userId DEL TOKEN
                    Activa = categoriaDto.Activa ?? true
                };

                var categoriaCreada = await _categoriasService.CrearAsync(categoria);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = categoriaCreada.CategoriaId }, MapToDto(categoriaCreada));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear categoría", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoriaTransaccionResponseDto>> Actualizar(int id, [FromBody] CategoriaTransaccionUpdateDto categoriaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                // Validar propiedad antes de actualizar
                var categoriaExistente = await _categoriasService.ObtenerPorIdAsync(id);
                if (categoriaExistente == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                // No permitir editar categorías de sistema ni de otros usuarios
                if (categoriaExistente.EsSistema == true)
                    return BadRequest(new { mensaje = "No se pueden modificar categorías del sistema" });

                if (categoriaExistente.UsuarioId != userId.Value)
                    return Forbid();

                var categoria = new CategoriasTransaccione
                {
                    Nombre = categoriaDto.Nombre,
                    Tipo = categoriaDto.Tipo,
                    Color = categoriaDto.Color,
                    Icono = categoriaDto.Icono,
                    Activa = categoriaDto.Activa
                };

                var categoriaActualizada = await _categoriasService.ActualizarAsync(id, categoria);
                return Ok(MapToDto(categoriaActualizada!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar categoría", error = ex.Message });
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
                var categoria = await _categoriasService.ObtenerPorIdAsync(id);
                if (categoria == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                // No permitir eliminar categorías de sistema ni de otros usuarios
                if (categoria.EsSistema == true)
                    return BadRequest(new { mensaje = "No se pueden eliminar categorías del sistema" });

                if (categoria.UsuarioId != userId.Value)
                    return Forbid();

                var resultado = await _categoriasService.EliminarAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Categoría no encontrada o es de sistema (no se puede eliminar)" });

                return Ok(new { mensaje = "Categoría eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar categoría", error = ex.Message });
            }
        }

        private CategoriaTransaccionResponseDto MapToDto(CategoriasTransaccione categoria)
        {
            return new CategoriaTransaccionResponseDto
            {
                CategoriaId = categoria.CategoriaId,
                Nombre = categoria.Nombre,
                Tipo = categoria.Tipo,
                Color = categoria.Color,
                Icono = categoria.Icono,
                EsSistema = categoria.EsSistema,
                UsuarioId = categoria.UsuarioId,
                Activa = categoria.Activa
            };
        }
    }
}
