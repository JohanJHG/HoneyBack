using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebasController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;
        private readonly IReportesService _reportesService;
        private readonly IMensajesContactoService _mensajesService;

        public PruebasController(
            IUsuariosService usuariosService,
            IReportesService reportesService,
            IMensajesContactoService mensajesService)
        {
            _usuariosService = usuariosService;
            _reportesService = reportesService;
            _mensajesService = mensajesService;
        }

        [HttpPost("datos-prueba")]
        public async Task<ActionResult> CrearDatosDePrueba()
        {
            try
            {
                // Crear usuarios de prueba
                var usuario1 = await _usuariosService.CrearAsync(new Usuario
                {
                    NombreCompleto = "María González",
                    Email = "maria@test.com",
                    PasswordHash = "hash123"
                });

                var usuario2 = await _usuariosService.CrearAsync(new Usuario
                {
                    NombreCompleto = "Carlos López",
                    Email = "carlos@test.com",
                    PasswordHash = "hash456"
                });

                // Crear reportes de prueba
                var reporte1 = await _reportesService.CrearAsync(new Reporte
                {
                    Nombre = "Reporte Mensual",
                    TipoReporte = "Mensual",
                    Descripcion = "Reporte de prueba del mes",
                    UsuarioId = usuario1.UsuarioId
                });

                var reporte2 = await _reportesService.CrearAsync(new Reporte
                {
                    Nombre = "Análisis Trimestral",
                    TipoReporte = "Trimestral",
                    Descripcion = "Análisis de datos del trimestre",
                    UsuarioId = usuario2.UsuarioId
                });

                // Crear mensajes de prueba
                var mensaje1 = await _mensajesService.CrearAsync(new MensajesContacto
                {
                    Nombre = "Pedro Martínez",
                    Email = "pedro@example.com",
                    Mensaje = "Consulta sobre el servicio"
                });

                return Ok(new
                {
                    mensaje = "Datos de prueba creados exitosamente",
                    usuarios = new[] { usuario1, usuario2 },
                    reportes = new[] { reporte1, reporte2 },
                    mensajes = new[] { mensaje1 }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear datos de prueba", error = ex.Message });
            }
        }

        [HttpGet("verificar-conexion")]
        public async Task<ActionResult> VerificarConexion()
        {
            try
            {
                var usuarios = await _usuariosService.ObtenerTodosAsync();
                var reportes = await _reportesService.ObtenerTodosAsync();
                var mensajes = await _mensajesService.ObtenerTodosAsync();

                return Ok(new
                {
                    mensaje = "Conexión exitosa",
                    totalUsuarios = usuarios.Count(),
                    totalReportes = reportes.Count(),
                    totalMensajes = mensajes.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error de conexión", error = ex.Message });
            }
        }

        [HttpDelete("limpiar-datos-prueba")]
        public async Task<ActionResult> LimpiarDatosDePrueba()
        {
            try
            {
                var usuarios = await _usuariosService.ObtenerTodosAsync();
                foreach (var usuario in usuarios.Where(u => u.Email.Contains("@test.com")))
                {
                    await _usuariosService.EliminarAsync(usuario.UsuarioId);
                }

                return Ok(new { mensaje = "Datos de prueba eliminados exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al limpiar datos", error = ex.Message });
            }
        }
    }
}
