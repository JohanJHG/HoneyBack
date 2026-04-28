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
    public class EntornosPersonalesController : ControllerBase
    {
        private readonly IEntornosPersonalesService _entornosService;
        private const int MaxRegistrosPorModulo = 50;

        private static readonly HashSet<string> ModulosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "presupuesto", "ahorro-meta", "deudas",
            "gastos-hormiga", "fondo-emergencia", "suscripciones"
        };

        public EntornosPersonalesController(IEntornosPersonalesService entornosService)
        {
            _entornosService = entornosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntornoPersonalResponseDto>>> ObtenerTodos()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var entornos = await _entornosService.ObtenerPorUsuarioAsync(userId.Value);
            return Ok(entornos.Select(MapToDto));
        }

        [HttpGet("modulo/{moduloClave}")]
        public async Task<ActionResult<IEnumerable<EntornoPersonalResponseDto>>> ObtenerPorModulo(string moduloClave)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (!ModulosValidos.Contains(moduloClave))
                return BadRequest(new { mensaje = $"Modulo '{moduloClave}' no es valido. Modulos validos: {string.Join(", ", ModulosValidos)}" });

            var entornos = await _entornosService.ObtenerPorUsuarioYModuloAsync(userId.Value, moduloClave);
            return Ok(entornos.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EntornoPersonalResponseDto>> ObtenerPorId(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var entorno = await _entornosService.ObtenerPorIdAsync(id);
            if (entorno == null)
                return NotFound(new { mensaje = "Entorno no encontrado" });

            if (entorno.UsuarioId != userId.Value)
                return Forbid();

            return Ok(MapToDto(entorno));
        }

        [HttpPost]
        public async Task<ActionResult<EntornoPersonalResponseDto>> Crear([FromBody] EntornoPersonalCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (!ModulosValidos.Contains(dto.ModuloClave))
                return BadRequest(new { mensaje = $"Modulo '{dto.ModuloClave}' no es valido" });

            var count = await _entornosService.ContarPorModuloAsync(userId.Value, dto.ModuloClave);
            if (count >= MaxRegistrosPorModulo)
                return BadRequest(new { mensaje = $"Limite de {MaxRegistrosPorModulo} registros por modulo alcanzado" });

            var entorno = new EntornoPersonal
            {
                UsuarioId = userId.Value,
                ModuloClave = dto.ModuloClave,
                Titulo = dto.Titulo,
                Subtitulo = dto.Subtitulo,
                ValorPrincipal = dto.ValorPrincipal,
                Etiqueta = dto.Etiqueta,
                DatosJson = dto.DatosJson
            };

            var creado = await _entornosService.CrearAsync(entorno);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.EntornoId }, MapToDto(creado));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EntornoPersonalResponseDto>> Actualizar(int id, [FromBody] EntornoPersonalUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var existente = await _entornosService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound(new { mensaje = "Entorno no encontrado" });

            if (existente.UsuarioId != userId.Value)
                return Forbid();

            var entorno = new EntornoPersonal
            {
                Titulo = dto.Titulo ?? existente.Titulo,
                Subtitulo = dto.Subtitulo ?? existente.Subtitulo,
                ValorPrincipal = dto.ValorPrincipal ?? existente.ValorPrincipal,
                Etiqueta = dto.Etiqueta ?? existente.Etiqueta,
                DatosJson = dto.DatosJson ?? existente.DatosJson
            };

            var actualizado = await _entornosService.ActualizarAsync(id, entorno);
            return Ok(MapToDto(actualizado!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var entorno = await _entornosService.ObtenerPorIdAsync(id);
            if (entorno == null)
                return NotFound(new { mensaje = "Entorno no encontrado" });

            if (entorno.UsuarioId != userId.Value)
                return Forbid();

            await _entornosService.EliminarAsync(id);
            return NoContent();
        }

        [HttpDelete("modulo/{moduloClave}")]
        public async Task<ActionResult> LimpiarModulo(string moduloClave)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            if (!ModulosValidos.Contains(moduloClave))
                return BadRequest(new { mensaje = $"Modulo '{moduloClave}' no es valido" });

            var eliminados = await _entornosService.EliminarTodosPorModuloAsync(userId.Value, moduloClave);
            return Ok(new { mensaje = $"{eliminados} registros eliminados", eliminados });
        }

        private static EntornoPersonalResponseDto MapToDto(EntornoPersonal entorno) => new()
        {
            EntornoId = entorno.EntornoId,
            UsuarioId = entorno.UsuarioId,
            ModuloClave = entorno.ModuloClave,
            Titulo = entorno.Titulo,
            Subtitulo = entorno.Subtitulo,
            ValorPrincipal = entorno.ValorPrincipal,
            Etiqueta = entorno.Etiqueta,
            DatosJson = entorno.DatosJson,
            FechaCreacion = entorno.FechaCreacion,
            FechaActualizacion = entorno.FechaActualizacion
        };
    }
}
