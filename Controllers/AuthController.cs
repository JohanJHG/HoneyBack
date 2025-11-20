using HoneyBack.DTOs.Auth;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net;

namespace HoneyBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;
        private readonly ISesionesService _sesionesService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            IUsuariosService usuariosService,
            ISesionesService sesionesService,
            IJwtTokenService jwtTokenService)
        {
            _usuariosService = usuariosService;
            _sesionesService = sesionesService;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Login de usuario con email y contraseña
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            // Buscar usuario por email
            var usuario = await _usuariosService.ObtenerPorEmailAsync(request.Email);
            if (usuario == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Verificar si la contraseña está hasheada con BCrypt o es texto plano
            bool passwordValida = false;
            bool requiereMigracion = false;

            try
            {
                // Intentar validar con BCrypt (contraseñas hasheadas)
                if (usuario.PasswordHash.StartsWith("$2"))
                {
                    passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
                }
                else
                {
                    // Contraseña en texto plano (legacy)
                    passwordValida = usuario.PasswordHash == request.Password;
                    requiereMigracion = true;
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Si falla la verificación BCrypt, comparar como texto plano
                passwordValida = usuario.PasswordHash == request.Password;
                requiereMigracion = true;
            }

            if (!passwordValida)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Si la contraseña requiere migración, actualizarla a BCrypt
            if (requiereMigracion)
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _usuariosService.ActualizarAsync(usuario.UsuarioId, usuario);
            }

            // Generar JWT token
            var (token, expiresAt) = _jwtTokenService.Generate(usuario);

            // Crear sesión en la base de datos
            var sesion = new Sesione
            {
                UsuarioId = usuario.UsuarioId,
                TokenSesion = token,
                FechaExpiracion = expiresAt
            };
            await _sesionesService.CrearAsync(sesion);

            // Limpiar sesiones expiradas (opcional, para mantener la BD limpia)
            await _sesionesService.LimpiarSesionesExpiradasAsync();

            // Devolver respuesta con token y datos del usuario
            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt.ToString("o"), // ISO 8601 format
                Usuario = new UsuarioDto
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.NombreCompleto) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Todos los campos son requeridos" });
            }

            // Validar formato de email
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "El formato del email no es válido" });
            }

            // Verificar si el email ya existe
            if (await _usuariosService.ExisteEmailAsync(request.Email))
            {
                return Conflict(new { message = "El email ya está registrado" });
            }

            // Validar longitud mínima de contraseña
            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            // Hashear contraseña con BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Crear usuario
            var nuevoUsuario = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                Email = request.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                FechaRegistro = DateTime.UtcNow
            };

            var usuarioCreado = await _usuariosService.CrearAsync(nuevoUsuario);

            // Devolver respuesta sin contraseña
            var response = new RegisterResponseDto
            {
                UsuarioId = usuarioCreado.UsuarioId,
                NombreCompleto = usuarioCreado.NombreCompleto,
                Email = usuarioCreado.Email,
                FechaRegistro = usuarioCreado.FechaRegistro
            };

            return CreatedAtAction(nameof(Register), new { id = usuarioCreado.UsuarioId }, response);
        }

        /// <summary>
        /// Obtener información del usuario autenticado actual
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> GetCurrentUser()
        {
            // Obtener el ID del usuario desde los claims del JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            // Buscar el usuario en la base de datos
            var usuario = await _usuariosService.ObtenerPorIdAsync(userId);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Devolver datos del usuario sin la contraseña
            var response = new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                FechaRegistro = usuario.FechaRegistro
            };

            return Ok(response);
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        [HttpPost("cambiar-password")]
        [Authorize]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordDto request)
        {
            // Obtener el ID del usuario desde los claims del JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.PasswordActual) || string.IsNullOrWhiteSpace(request.PasswordNueva))
            {
                return BadRequest(new { message = "Contraseña actual y nueva son requeridas" });
            }

            // Validar longitud mínima de contraseña nueva
            if (request.PasswordNueva.Length < 6)
            {
                return BadRequest(new { message = "La contraseña nueva debe tener al menos 6 caracteres" });
            }

            // Buscar el usuario
            var usuario = await _usuariosService.ObtenerPorIdAsync(userId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Verificar que la contraseña actual es correcta
            bool passwordValida = BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash);
            if (!passwordValida)
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }

            // Hashear la nueva contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNueva);

            // Actualizar el usuario
            await _usuariosService.ActualizarAsync(userId, usuario);

            return Ok(new { message = "Contraseña actualizada exitosamente" });
        }

        /// <summary>
        /// Cerrar sesión (invalidar token actual)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            // Obtener el token del header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token no proporcionado" });
            }

            // Buscar y eliminar la sesión
            var sesion = await _sesionesService.ObtenerPorTokenAsync(token);
            if (sesion != null)
            {
                await _sesionesService.EliminarAsync(sesion.SesionId);
            }

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
