using System.Security.Claims;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HoneyBalanceDbContext _db;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(HoneyBalanceDbContext db, IJwtTokenService jwtTokenService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public object User { get; set; } = default!;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { mensaje = "Email y contraseña son requeridos" });

            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            // NOTA: Temporalmente comparamos texto plano (migrar a BCrypt ASAP)
            var passwordOk = user.PasswordHash == request.Password;
            if (!passwordOk)
                return Unauthorized(new { mensaje = "Contraseña incorrecta" });

            var (token, expiresAt) = _jwtTokenService.Generate(user);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new { id = user.UsuarioId, nombreCompleto = user.NombreCompleto, email = user.Email }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> Me()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(sub))
                return Unauthorized(new { mensaje = "Token inválido" });

            if (!int.TryParse(sub, out int userId))
                return Unauthorized(new { mensaje = "Token inválido" });

            var user = await _db.Usuarios.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado" });

            return Ok(new { id = user.UsuarioId, nombreCompleto = user.NombreCompleto, email = user.Email });
        }

        // Opcional: para compatibilidad con validaciones de front antiguo
        [HttpPost("validate")]
        [AllowAnonymous]
        public ActionResult ValidateToken([FromBody] string token)
        {
            // Si llega aquí, puedes implementar validación manual, o el front debería usar /auth/me con Bearer
            return Ok(new { mensaje = "Use Authorization: Bearer {token} y llame a /api/auth/me" });
        }
    }
}
