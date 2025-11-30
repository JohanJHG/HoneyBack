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
    public class CategoriasTransaccionesController : ControllerBase
    {
        private readonly ICategoriasTransaccionesService _categoriasService;

        public CategoriasTransaccionesController(ICategoriasTransaccionesService categoriasService)
        {
            _categoriasService = categoriasService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerTodos()
        {
            try
            {
                var categorias = await _categoriasService.ObtenerTodosAsync();
                var response = categorias.Select(c => new CategoriaTransaccionResponseDto
                {
                    CategoriaId = c.CategoriaId,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Color = c.Color,
                    Icono = c.Icono,
                    EsSistema = c.EsSistema,
                    UsuarioId = c.UsuarioId,
                    Activa = c.Activa
                });

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
                var categoria = await _categoriasService.ObtenerPorIdAsync(id);
                if (categoria == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                var response = new CategoriaTransaccionResponseDto
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

                return Ok(response);
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
                var categorias = await _categoriasService.ObtenerPorUsuarioAsync(usuarioId);
                var response = categorias.Select(c => new CategoriaTransaccionResponseDto
                {
                    CategoriaId = c.CategoriaId,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Color = c.Color,
                    Icono = c.Icono,
                    EsSistema = c.EsSistema,
                    UsuarioId = c.UsuarioId,
                    Activa = c.Activa
                });

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
                var categorias = await _categoriasService.ObtenerPorTipoAsync(tipo);
                var response = categorias.Select(c => new CategoriaTransaccionResponseDto
                {
                    CategoriaId = c.CategoriaId,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Color = c.Color,
                    Icono = c.Icono,
                    EsSistema = c.EsSistema,
                    UsuarioId = c.UsuarioId,
                    Activa = c.Activa
                });

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
                var categorias = await _categoriasService.ObtenerCategoriasActivasAsync(usuarioId);
                var response = categorias.Select(c => new CategoriaTransaccionResponseDto
                {
                    CategoriaId = c.CategoriaId,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Color = c.Color,
                    Icono = c.Icono,
                    EsSistema = c.EsSistema,
                    UsuarioId = c.UsuarioId,
                    Activa = c.Activa
                });

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

                // Validar si ya existe
                if (categoriaDto.UsuarioId.HasValue && 
                    await _categoriasService.ExisteNombreAsync(categoriaDto.Nombre, categoriaDto.UsuarioId.Value, categoriaDto.Tipo))
                {
                    return Conflict(new { mensaje = "Ya existe una categoría con ese nombre para el usuario" });
                }

                var categoria = new CategoriasTransaccione
                {
                    Nombre = categoriaDto.Nombre,
                    Tipo = categoriaDto.Tipo,
                    Color = categoriaDto.Color ?? "#FFD8A9",
                    Icono = categoriaDto.Icono,
                    EsSistema = categoriaDto.EsSistema ?? false,
                    UsuarioId = categoriaDto.UsuarioId,
                    Activa = categoriaDto.Activa ?? true
                };

                var categoriaCreada = await _categoriasService.CrearAsync(categoria);

                var response = new CategoriaTransaccionResponseDto
                {
                    CategoriaId = categoriaCreada.CategoriaId,
                    Nombre = categoriaCreada.Nombre,
                    Tipo = categoriaCreada.Tipo,
                    Color = categoriaCreada.Color,
                    Icono = categoriaCreada.Icono,
                    EsSistema = categoriaCreada.EsSistema,
                    UsuarioId = categoriaCreada.UsuarioId,
                    Activa = categoriaCreada.Activa
                };

                return CreatedAtAction(nameof(ObtenerPorId), new { id = categoriaCreada.CategoriaId }, response);
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

                var categoria = new CategoriasTransaccione
                {
                    Nombre = categoriaDto.Nombre,
                    Tipo = categoriaDto.Tipo,
                    Color = categoriaDto.Color,
                    Icono = categoriaDto.Icono,
                    Activa = categoriaDto.Activa
                };

                var categoriaActualizada = await _categoriasService.ActualizarAsync(id, categoria);
                if (categoriaActualizada == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                var response = new CategoriaTransaccionResponseDto
                {
                    CategoriaId = categoriaActualizada.CategoriaId,
                    Nombre = categoriaActualizada.Nombre,
                    Tipo = categoriaActualizada.Tipo,
                    Color = categoriaActualizada.Color,
                    Icono = categoriaActualizada.Icono,
                    EsSistema = categoriaActualizada.EsSistema,
                    UsuarioId = categoriaActualizada.UsuarioId,
                    Activa = categoriaActualizada.Activa
                };

                return Ok(response);
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
    }
}
