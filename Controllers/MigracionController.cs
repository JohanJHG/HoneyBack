using HoneyBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MigracionController : ControllerBase
    {
        private readonly HoneyBalanceDbContext _context;
        private readonly IConfiguration _configuration;

        public MigracionController(HoneyBalanceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// ENDPOINT TEMPORAL: Migrar contraseñas en texto plano a BCrypt
        /// ELIMINAR en producción después de ejecutar
        /// </summary>
        [HttpPost("hashear-passwords")]
        public async Task<IActionResult> HashearPasswordsExistentes([FromHeader(Name = "X-Migration-Key")] string? migrationKey)
        {
            // Protección: Solo permitir en desarrollo o con clave de migración
            var expectedKey = _configuration["MigrationKey"];
            if (!string.Equals(migrationKey, expectedKey, StringComparison.Ordinal))
            {
                return Unauthorized(new { message = "Clave de migración inválida" });
            }

            var usuarios = await _context.Usuarios.ToListAsync();
            int actualizados = 0;

            foreach (var usuario in usuarios)
            {
                // Verificar si la contraseña ya está hasheada (BCrypt genera hashes que empiezan con "$2a$" o "$2b$")
                if (!usuario.PasswordHash.StartsWith("$2"))
                {
                    // Hashear la contraseña en texto plano
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                    actualizados++;
                }
            }

            if (actualizados > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Migración completada. {actualizados} contraseñas hasheadas de {usuarios.Count} usuarios totales.",
                usuariosTotales = usuarios.Count,
                passwordsActualizadas = actualizados
            });
        }
    }
}
