using HoneyBack.DTOs.Auth;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly IEmailService _emailService;
        private readonly HoneyBalanceDbContext _context;
        private readonly ILogger<AuthController> _logger;

        // Duración del token de recuperación en minutos
        private const int TokenExpirationMinutes = 15;

        public AuthController(
            IUsuariosService usuariosService,
            ISesionesService sesionesService,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            HoneyBalanceDbContext context,
            ILogger<AuthController> logger)
        {
            _usuariosService = usuariosService;
            _sesionesService = sesionesService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
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

        /// <summary>
        /// Solicita un código de recuperación de contraseña.
        /// Siempre devuelve respuesta genérica para evitar enumeración de emails.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            // Respuesta genérica por seguridad (no revela si el email existe)
            var genericResponse = new { mensaje = "Si el email esta registrado, recibiras un codigo de recuperacion en tu correo." };

            try
            {
                // Buscar usuario por email (case-insensitive)
                var usuario = await _usuariosService.ObtenerPorEmailAsync(request.Email.ToLower().Trim());

                // Si no existe, retornar respuesta genérica (seguridad: no revelar si email existe)
                if (usuario == null)
                {
                    _logger.LogInformation("Intento de recuperacion para email no registrado: {Email}", request.Email);
                    return Ok(genericResponse);
                }

                // Invalidar tokens anteriores no usados del usuario
                var tokensAnteriores = await _context.PasswordResetTokens
                    .Where(t => t.UsuarioId == usuario.UsuarioId && !t.Used)
                    .ToListAsync();

                foreach (var tokenAnterior in tokensAnteriores)
                {
                    tokenAnterior.Used = true;
                }

                // Generar código de 6 dígitos criptográficamente seguro
                var token = GenerateSecureToken();

                // Crear registro de token
                var resetToken = new PasswordResetToken
                {
                    UsuarioId = usuario.UsuarioId,
                    Token = token,
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes),
                    Used = false
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                // Enviar email con el código
                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    usuario.Email,
                    usuario.NombreCompleto ?? usuario.Email,
                    token
                );

                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar email de recuperacion a {Email}", usuario.Email);
                }

                _logger.LogInformation(
                    "Codigo de recuperacion generado para UsuarioId: {UserId}. Email enviado: {EmailSent}",
                    usuario.UsuarioId, emailSent);

                return Ok(genericResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ForgotPassword para {Email}", request.Email);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Restablece la contraseña usando el código de verificación.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                // Buscar token válido (no usado y no expirado)
                var resetToken = await _context.PasswordResetTokens
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t =>
                        t.Token == request.Token &&
                        !t.Used &&
                        t.ExpiresAtUtc > DateTime.UtcNow);

                // Validar token
                if (resetToken == null)
                {
                    _logger.LogWarning("Intento de reset con token invalido o expirado: {Token}", request.Token);
                    return BadRequest(new { mensaje = "El codigo es invalido o ha expirado. Solicita uno nuevo." });
                }

                // Validar que el usuario existe
                if (resetToken.Usuario == null)
                {
                    _logger.LogWarning("Token {Token} asociado a usuario inexistente", request.Token);
                    return BadRequest(new { mensaje = "Usuario no encontrado" });
                }

                // Hash de la nueva contraseña con BCrypt (work factor 12)
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);

                // Actualizar contraseña del usuario
                resetToken.Usuario.PasswordHash = hashedPassword;
                resetToken.Usuario.FechaUltimaActualizacion = DateTime.UtcNow;

                // Marcar token como usado
                resetToken.Used = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Contrasena restablecida exitosamente para UsuarioId: {UserId}",
                    resetToken.UsuarioId);

                return Ok(new { mensaje = "Contrasena restablecida exitosamente. Ya puedes iniciar sesion." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ResetPassword");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Genera un código numérico de 6 dígitos usando un generador criptográficamente seguro.
        /// </summary>
        private static string GenerateSecureToken()
        {
            // Usar RandomNumberGenerator para seguridad criptográfica
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            
            // Convertir a número positivo y tomar módulo para 6 dígitos
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000 + 100000;
            return number.ToString();
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
