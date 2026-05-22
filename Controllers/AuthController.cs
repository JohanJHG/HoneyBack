using Google.Apis.Auth;
using HoneyBack.DTOs.Auth;
using HoneyBack.Extensions;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        private const int TokenExpirationMinutes = 15;
        private static readonly string SpecialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

        public AuthController(
            IUsuariosService usuariosService,
            ISesionesService sesionesService,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            HoneyBalanceDbContext context,
            ILogger<AuthController> logger,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _usuariosService = usuariosService;
            _sesionesService = sesionesService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
            _env = env;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var usuario = await _usuariosService.ObtenerPorEmailAsync(request.Email);
            if (usuario == null)
            {
                _logger.LogWarning("Login fallido: email no encontrado ip={IP}", ip);
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            bool passwordValida = false;
            bool requiereMigracion = false;

            try
            {
                if (usuario.PasswordHash.StartsWith("$2"))
                {
                    passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
                }
                else
                {
                    passwordValida = usuario.PasswordHash == request.Password;
                    requiereMigracion = true;
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                _logger.LogWarning("Hash corrupto (SaltParseException): usuarioId={UserId} ip={IP}", usuario.UsuarioId, ip);
                passwordValida = usuario.PasswordHash == request.Password;
                requiereMigracion = true;
            }

            if (!passwordValida)
            {
                _logger.LogWarning("Login fallido: contraseña inválida usuarioId={UserId} ip={IP}", usuario.UsuarioId, ip);
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            if (requiereMigracion)
            {
                _logger.LogInformation("Migrando hash de contraseña a BCrypt: usuarioId={UserId}", usuario.UsuarioId);
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _usuariosService.ActualizarAsync(usuario.UsuarioId, usuario);
            }

            var (token, expiresAt) = _jwtTokenService.Generate(usuario);

            var sesion = new Sesione
            {
                UsuarioId = usuario.UsuarioId,
                TokenSesion = token,
                FechaExpiracion = DateTime.SpecifyKind(expiresAt, DateTimeKind.Unspecified),
                Ipaddress = ip
            };
            await _sesionesService.CrearAsync(sesion);
            await _sesionesService.LimpiarSesionesExpiradasAsync();

            // Emitir refresh token en HttpOnly cookie (no accesible desde JS)
            var refreshTokenValue = GenerarRefreshToken();
            _context.RefreshTokens.Add(new HoneyBack.Models.RefreshToken
            {
                UsuarioId = usuario.UsuarioId,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            Response.Cookies.Append("refresh_token", refreshTokenValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            _logger.LogInformation("Login exitoso: usuarioId={UserId} ip={IP}", usuario.UsuarioId, ip);

            return Ok(new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt.ToString("o"),
                RefreshToken = refreshTokenValue,
                Usuario = new UsuarioDto
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro
                }
            });
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!IsValidEmail(request.Email))
                return BadRequest(new { message = "El formato del email no es válido" });

            if (await _usuariosService.ExisteEmailAsync(request.Email))
                return Conflict(new { message = "El email ya está registrado" });

            if (!EsPasswordSeguro(request.Password, out var razon))
                return BadRequest(new { message = razon });

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                Email = request.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                FechaRegistro = DateTime.UtcNow
            };

            var usuarioCreado = await _usuariosService.CrearAsync(nuevoUsuario);
            _logger.LogInformation("Registro exitoso: usuarioId={UserId} ip={IP}", usuarioCreado.UsuarioId, ip);

            return CreatedAtAction(nameof(Register), new { id = usuarioCreado.UsuarioId }, new RegisterResponseDto
            {
                UsuarioId = usuarioCreado.UsuarioId,
                NombreCompleto = usuarioCreado.NombreCompleto,
                Email = usuarioCreado.Email,
                FechaRegistro = usuarioCreado.FechaRegistro
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> GetCurrentUser()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Token inválido" });

            var usuario = await _usuariosService.ObtenerPorIdAsync(userId.Value);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                FechaRegistro = usuario.FechaRegistro
            });
        }

        [HttpPost("cambiar-password")]
        [Authorize]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Token inválido" });

            if (request.PasswordNueva != request.ConfirmarPasswordNueva)
                return BadRequest(new { message = "La nueva contraseña y su confirmación no coinciden" });

            if (!EsPasswordSeguro(request.PasswordNueva, out var razon))
                return BadRequest(new { message = razon });

            var usuario = await _usuariosService.ObtenerPorIdAsync(userId.Value);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (!BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash))
            {
                _logger.LogWarning("Cambio de contraseña fallido: contraseña actual incorrecta. usuarioId={UserId}", userId.Value);
                return Unauthorized(new { message = "La contraseña actual es incorrecta" });
            }

            if (BCrypt.Net.BCrypt.Verify(request.PasswordNueva, usuario.PasswordHash))
                return BadRequest(new { message = "La nueva contraseña no puede ser igual a la actual" });

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNueva);
            await _usuariosService.ActualizarAsync(userId.Value, usuario);

            _logger.LogInformation("Contraseña cambiada: usuarioId={UserId}", userId.Value);
            return Ok(new { message = "Contraseña actualizada exitosamente" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var userId = User.GetUserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token no proporcionado" });

            var sesion = await _sesionesService.ObtenerPorTokenAsync(token);
            if (sesion != null)
                await _sesionesService.EliminarAsync(sesion.SesionId);

            _logger.LogInformation("Logout: usuarioId={UserId} ip={IP}", userId, ip);
            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? request)
        {
            // Acepta refresh token desde cookie HttpOnly (prod) o desde body (dev)
            var tokenValue = Request.Cookies["refresh_token"] ?? request?.RefreshToken;
            if (string.IsNullOrWhiteSpace(tokenValue))
                return Unauthorized(new { message = "Refresh token no proporcionado." });

            var stored = await _context.RefreshTokens
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Token == tokenValue && !r.IsRevoked);

            if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token inválido o expirado. ip={IP}", HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Refresh token inválido o expirado." });
            }

            var nuevoRefreshToken = GenerarRefreshToken();
            stored.IsRevoked = true;
            stored.ReplacedByToken = nuevoRefreshToken;

            var nuevoAccessToken = _jwtTokenService.Generate(stored.Usuario);

            _context.RefreshTokens.Add(new HoneyBack.Models.RefreshToken
            {
                UsuarioId = stored.UsuarioId,
                Token = nuevoRefreshToken,
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            // Refresh token en HttpOnly cookie; access token en body
            Response.Cookies.Append("refresh_token", nuevoRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                accessToken = nuevoAccessToken.token,
                expiresAt = nuevoAccessToken.expiresAt.ToString("o"),
                refreshToken = nuevoRefreshToken
            });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            var genericResponse = new { message = "Si el email está registrado, recibirás un código de recuperación en tu correo." };

            var usuario = await _usuariosService.ObtenerPorEmailAsync(request.Email.ToLower().Trim());
            if (usuario == null)
            {
                _logger.LogInformation("Recuperación solicitada para email no registrado: ip={IP}", HttpContext.Connection.RemoteIpAddress);
                return Ok(genericResponse);
            }

            var tokensAnteriores = await _context.PasswordResetTokens
                .Where(t => t.UsuarioId == usuario.UsuarioId && !t.Used)
                .ToListAsync();

            foreach (var tokenAnterior in tokensAnteriores)
                tokenAnterior.Used = true;

            var token = GenerateSecureToken();
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UsuarioId = usuario.UsuarioId,
                Token = token,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(TokenExpirationMinutes),
                Used = false
            });
            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                usuario.Email,
                usuario.NombreCompleto ?? usuario.Email,
                token);

            if (emailSent)
                _logger.LogInformation("Código de recuperación generado y enviado: usuarioId={UserId}", usuario.UsuarioId);
            else
                _logger.LogWarning("Código de recuperación generado pero email NO enviado: usuarioId={UserId}", usuario.UsuarioId);
            return Ok(genericResponse);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            if (!EsPasswordSeguro(request.NewPassword, out var razon))
                return BadRequest(new { message = razon });

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t =>
                    t.Token == request.Token &&
                    !t.Used &&
                    t.ExpiresAtUtc > now);

            if (resetToken == null)
            {
                _logger.LogWarning("Intento de reset con token inválido o expirado");
                return BadRequest(new { message = "El código es inválido o ha expirado. Solicita uno nuevo." });
            }

            if (resetToken.Usuario == null)
                return BadRequest(new { message = "Usuario no encontrado" });

            resetToken.Usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            resetToken.Usuario.FechaUltimaActualizacion = now;
            resetToken.Used = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contraseña restablecida: usuarioId={UserId}", resetToken.UsuarioId);
            return Ok(new { message = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión." });
        }

        [HttpPost("google")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<LoginResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ObtenerPrimerError(ModelState) });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                var clientId = _configuration["Google:ClientId"]
                    ?? throw new InvalidOperationException("Google:ClientId no configurado");

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                var usuario = await _usuariosService.ObtenerPorEmailAsync(payload.Email);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        NombreCompleto = payload.Name ?? payload.Email,
                        Email = payload.Email.ToLower().Trim(),
                        PasswordHash = "GOOGLE_OAUTH",
                        FechaRegistro = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        AvatarUrl = payload.Picture,
                        Activo = true
                    };
                    usuario = await _usuariosService.CrearAsync(usuario);
                    _logger.LogInformation("Usuario creado via Google OAuth: usuarioId={UserId} ip={IP}", usuario.UsuarioId, ip);
                }

                var (token, expiresAt) = _jwtTokenService.Generate(usuario);

                var sesion = new Sesione
                {
                    UsuarioId = usuario.UsuarioId,
                    TokenSesion = token,
                    FechaExpiracion = DateTime.SpecifyKind(expiresAt, DateTimeKind.Unspecified),
                    Ipaddress = ip
                };
                await _sesionesService.CrearAsync(sesion);
                await _sesionesService.LimpiarSesionesExpiradasAsync();

                var refreshTokenValue = GenerarRefreshToken();
                _context.RefreshTokens.Add(new HoneyBack.Models.RefreshToken
                {
                    UsuarioId = usuario.UsuarioId,
                    Token = refreshTokenValue,
                    ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    IsRevoked = false
                });
                await _context.SaveChangesAsync();

                Response.Cookies.Append("refresh_token", refreshTokenValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                _logger.LogInformation("Google login exitoso: usuarioId={UserId} ip={IP}", usuario.UsuarioId, ip);

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    ExpiresAt = expiresAt.ToString("o"),
                    RefreshToken = refreshTokenValue,
                    Usuario = new UsuarioDto
                    {
                        UsuarioId = usuario.UsuarioId,
                        NombreCompleto = usuario.NombreCompleto,
                        Email = usuario.Email,
                        FechaRegistro = usuario.FechaRegistro
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning("Token de Google inválido: {Msg} ip={IP}", ex.Message, ip);
                return Unauthorized(new { message = "Token de Google inválido o expirado" });
            }
        }

        private static bool EsPasswordSeguro(string password, out string mensaje)
        {
            if (password.Length < 8)
            {
                mensaje = "La contraseña debe tener al menos 8 caracteres";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                mensaje = "La contraseña debe contener al menos una letra mayúscula";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                mensaje = "La contraseña debe contener al menos un número";
                return false;
            }
            if (!password.Any(c => SpecialChars.Contains(c)))
            {
                mensaje = "La contraseña debe contener al menos un carácter especial (!@#$%^&*...)";
                return false;
            }
            mensaje = string.Empty;
            return true;
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000 + 100000;
            return number.ToString();
        }

        private static string GenerarRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static bool IsValidEmail(string email)
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

        private static string ObtenerPrimerError(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            return modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Datos inválidos";
        }
    }
}
