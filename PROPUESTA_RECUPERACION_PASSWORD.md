# 🔐 Propuesta: Integración de Recuperación de Contraseña

## HoneyBalance - Documentación Técnica

**Fecha:** 8 de enero de 2026  
**Versión:** 1.0  
**Autor:** GitHub Copilot

---

## 📋 Tabla de Contenidos

1. [Resumen del Estado Actual](#-resumen-del-estado-actual)
2. [Arquitectura del Flujo](#-arquitectura-del-flujo-completo)
3. [Requisitos del Backend](#-requisitos-para-el-backend-net)
4. [Servicio de Email](#-servicio-de-email)
5. [Pasos de Implementación](#-pasos-para-implementar)
6. [Consideraciones de Seguridad](#-consideraciones-de-seguridad)
7. [Estructura de Archivos](#-archivos-a-crearmodificar)
8. [Código Completo](#-código-completo-del-backend)

---

## 📊 Resumen del Estado Actual

### Frontend (Angular) - ✅ COMPLETADO

| Componente                     | Estado             | Ubicación                                      |
| ------------------------------ | ------------------ | ---------------------------------------------- |
| `ForgotPasswordModalComponent` | ✅ Completo        | `src/app/layout/modals/forgot-password-modal/` |
| `AuthService.forgotPassword()` | ✅ Método listo    | `src/app/core/services/auth.service.ts`        |
| `AuthService.resetPassword()`  | ✅ Método listo    | `src/app/core/services/auth.service.ts`        |
| Modelos TypeScript             | ✅ Definidos       | `src/app/core/models/auth.models.ts`           |
| Integración en Login Modal     | ✅ Botón funcional | `src/app/layout/modals/login-modal/`           |

### Backend (.NET) - ❌ PENDIENTE

| Componente                                | Estado       | Acción Requerida |
| ----------------------------------------- | ------------ | ---------------- |
| Modelo `PasswordResetToken`               | ❌ Pendiente | Crear            |
| Endpoint `POST /api/auth/forgot-password` | ❌ Pendiente | Crear            |
| Endpoint `POST /api/auth/reset-password`  | ❌ Pendiente | Crear            |
| Servicio de Email                         | ❌ Pendiente | Configurar       |
| Migración de Base de Datos                | ❌ Pendiente | Ejecutar         |

---

## 🏗️ Arquitectura del Flujo Completo

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE RECUPERACIÓN DE CONTRASEÑA                       │
└─────────────────────────────────────────────────────────────────────────────┘

Usuario                   Frontend (Angular)              Backend (.NET)              Email Service
   │                            │                              │                           │
   │  1. Click "Olvidé         │                              │                           │
   │     mi contraseña"        │                              │                           │
   │ ─────────────────────────>│                              │                           │
   │                            │                              │                           │
   │                            │  2. POST /api/auth/          │                           │
   │                            │     forgot-password          │                           │
   │                            │     { email }                │                           │
   │                            │ ────────────────────────────>│                           │
   │                            │                              │                           │
   │                            │                              │  3. Generar token         │
   │                            │                              │     (6 dígitos)           │
   │                            │                              │     Guardar en DB         │
   │                            │                              │     con expiración        │
   │                            │                              │                           │
   │                            │                              │ ─────────────────────────>│
   │                            │                              │                           │  4. Enviar email
   │                            │                              │                           │     con código
   │<──────────────────────────────────────────────────────────────────────────────────────│
   │                            │                              │                           │
   │  5. Usuario recibe email con código de 6 dígitos          │                           │
   │                            │                              │                           │
   │  6. Ingresa código +      │                              │                           │
   │     nueva contraseña      │                              │                           │
   │ ─────────────────────────>│                              │                           │
   │                            │                              │                           │
   │                            │  7. POST /api/auth/          │                           │
   │                            │     reset-password           │                           │
   │                            │     { token, newPassword }   │                           │
   │                            │ ────────────────────────────>│                           │
   │                            │                              │                           │
   │                            │                              │  8. Validar token         │
   │                            │                              │     Hash nueva password   │
   │                            │                              │     Actualizar usuario    │
   │                            │                              │     Invalidar token       │
   │                            │                              │                           │
   │                            │  9. { success: true }        │                           │
   │                            │<────────────────────────────│                           │
   │                            │                              │                           │
   │  10. Mostrar mensaje de   │                              │                           │
   │      éxito y redirigir    │                              │                           │
   │      a login              │                              │                           │
   │<─────────────────────────│                              │                           │
```

### Diagrama de Estados del Modal (Frontend)

```
┌──────────────┐    submit email    ┌──────────────┐    submit code    ┌──────────────┐
│              │ ─────────────────> │              │ ────────────────> │              │
│  STEP 1:     │                    │  STEP 2:     │                   │  STEP 3:     │
│  Email Input │                    │  Code Input  │                   │  Success     │
│              │ <───────────────── │              │                   │              │
└──────────────┘    back button     └──────────────┘                   └──────────────┘
```

---

## 🔧 Requisitos para el Backend (.NET)

### 1. Modelo de Datos

#### PasswordResetToken.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoneyBalance.API.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(10)]
        public string Token { get; set; }  // Código de 6 dígitos

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool Used { get; set; } = false;

        // Navigation property
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
    }
}
```

### 2. DTOs (Data Transfer Objects)

#### ForgotPasswordRequest.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace HoneyBalance.API.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }
    }
}
```

#### ResetPasswordRequest.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace HoneyBalance.API.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }
    }
}
```

### 3. Actualizar DbContext

```csharp
// En ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    // ... otros DbSets existentes ...

    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UsuarioId);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### 4. Migración de Base de Datos

```bash
# En la terminal, dentro del proyecto backend
dotnet ef migrations add AddPasswordResetTokens
dotnet ef database update
```

---

## 📧 Servicio de Email

### Opciones de Proveedores

| Proveedor      | Costo       | Límite Gratis            | Complejidad | Recomendación               |
| -------------- | ----------- | ------------------------ | ----------- | --------------------------- |
| **SendGrid**   | Gratis      | 100 emails/día           | Baja        | ⭐ Recomendado para iniciar |
| **Mailgun**    | Gratis      | 5,000/mes (3 meses)      | Baja        | Alternativa                 |
| **AWS SES**    | ~$0.10/1000 | 62,000/mes (si usas EC2) | Media       | Producción escalable        |
| **SMTP Gmail** | Gratis      | 500/día                  | Media       | Solo desarrollo             |
| **Resend**     | Gratis      | 3,000/mes                | Muy baja    | Moderna y simple            |

### Configuración con SendGrid

#### 1. Instalar Paquete NuGet

```bash
dotnet add package SendGrid
```

#### 2. Configuración en appsettings.json

```json
{
  "SendGrid": {
    "ApiKey": "SG.xxxxxxxxxxxxxxxxxxxx",
    "FromEmail": "noreply@honeybalance.com",
    "FromName": "HoneyBalance"
  }
}
```

#### 3. Interfaz del Servicio

```csharp
// IEmailService.cs
namespace HoneyBalance.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token);
    }
}
```

#### 4. Implementación del Servicio

```csharp
// EmailService.cs
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace HoneyBalance.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridClient _client;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
            _fromName = configuration["SendGrid:FromName"];
            _client = new SendGridClient(apiKey);
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string token)
        {
            try
            {
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail, userName);
                var subject = "🔐 Recupera tu contraseña - HoneyBalance";

                var htmlContent = GeneratePasswordResetEmailTemplate(userName, token);
                var plainTextContent = $"Hola {userName}, tu código de recuperación es: {token}. Este código expira en 15 minutos.";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email de recuperación enviado exitosamente a {Email}", toEmail);
                    return true;
                }

                _logger.LogWarning("Error al enviar email: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de recuperación a {Email}", toEmail);
                return false;
            }
        }

        private string GeneratePasswordResetEmailTemplate(string userName, string token)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #1a1a1a;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                <table role='presentation' style='width: 600px; max-width: 100%; border-collapse: collapse; background-color: #0F0F0F; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.3);'>

                    <!-- Header -->
                    <tr>
                        <td style='padding: 40px 40px 20px; text-align: center; background: linear-gradient(135deg, #FFD8A9 0%, #E8A87C 100%);'>
                            <h1 style='margin: 0; font-size: 28px; font-weight: 700; color: #0F0F0F;'>
                                🍯 HoneyBalance
                            </h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            <h2 style='margin: 0 0 20px; font-size: 24px; font-weight: 600; color: #F9F9F9;'>
                                Hola {userName},
                            </h2>
                            <p style='margin: 0 0 30px; font-size: 16px; line-height: 1.6; color: #CCCCCC;'>
                                Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                                Usa el siguiente código para completar el proceso:
                            </p>

                            <!-- Code Box -->
                            <div style='background: linear-gradient(135deg, #1a1a1a 0%, #252525 100%); border: 2px solid #FFD8A9; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0;'>
                                <p style='margin: 0 0 10px; font-size: 14px; color: #CCCCCC; text-transform: uppercase; letter-spacing: 2px;'>
                                    Tu código de verificación
                                </p>
                                <h1 style='margin: 0; font-size: 48px; font-weight: 700; color: #FFD8A9; letter-spacing: 12px; font-family: monospace;'>
                                    {token}
                                </h1>
                                <p style='margin: 15px 0 0; font-size: 13px; color: #999999;'>
                                    ⏱️ Este código expira en <strong style='color: #FFD8A9;'>15 minutos</strong>
                                </p>
                            </div>

                            <p style='margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #999999;'>
                                Si no solicitaste restablecer tu contraseña, puedes ignorar este correo de forma segura.
                                Tu contraseña actual permanecerá sin cambios.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='padding: 30px 40px; background-color: #0a0a0a; border-top: 1px solid #2a2a2a;'>
                            <p style='margin: 0 0 10px; font-size: 12px; color: #666666; text-align: center;'>
                                Este es un correo automático, por favor no respondas a este mensaje.
                            </p>
                            <p style='margin: 0; font-size: 12px; color: #666666; text-align: center;'>
                                © 2026 HoneyBalance. Todos los derechos reservados.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
```

#### 5. Registrar Servicio en Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<IEmailService, EmailService>();
```

---

## 📝 Pasos para Implementar

### Fase 1: Preparación (10 min)

- [ ] 1.1 Crear cuenta en SendGrid (https://sendgrid.com)
- [ ] 1.2 Verificar dominio/email del remitente
- [ ] 1.3 Obtener API Key de SendGrid
- [ ] 1.4 Agregar API Key a `appsettings.json` (usar User Secrets para desarrollo)

### Fase 2: Base de Datos (10 min)

- [ ] 2.1 Crear modelo `PasswordResetToken.cs`
- [ ] 2.2 Agregar DbSet al contexto
- [ ] 2.3 Crear migración: `dotnet ef migrations add AddPasswordResetTokens`
- [ ] 2.4 Aplicar migración: `dotnet ef database update`

### Fase 3: Servicios (20 min)

- [ ] 3.1 Crear interfaz `IEmailService.cs`
- [ ] 3.2 Implementar `EmailService.cs`
- [ ] 3.3 Registrar servicio en `Program.cs`
- [ ] 3.4 Crear DTOs: `ForgotPasswordRequest.cs` y `ResetPasswordRequest.cs`

### Fase 4: Endpoints (30 min)

- [ ] 4.1 Agregar endpoint `POST /api/auth/forgot-password`
- [ ] 4.2 Agregar endpoint `POST /api/auth/reset-password`
- [ ] 4.3 Implementar validaciones
- [ ] 4.4 Implementar rate limiting (opcional pero recomendado)

### Fase 5: Testing (20 min)

- [ ] 5.1 Probar `forgot-password` con Postman/Thunder Client
- [ ] 5.2 Verificar recepción de email
- [ ] 5.3 Probar `reset-password` con código válido
- [ ] 5.4 Probar con código expirado/inválido
- [ ] 5.5 Probar flujo completo desde Angular

### Fase 6: Producción (15 min)

- [ ] 6.1 Configurar variables de entorno en servidor
- [ ] 6.2 Verificar dominio en SendGrid para producción
- [ ] 6.3 Configurar rate limiting en producción
- [ ] 6.4 Monitorear logs de emails

---

## 🔒 Consideraciones de Seguridad

### Implementación Obligatoria

| Aspecto                            | Descripción                  | Implementación                                               |
| ---------------------------------- | ---------------------------- | ------------------------------------------------------------ |
| **No revelar existencia de email** | Evitar enumeration attacks   | Siempre responder: "Si el email existe, recibirás un código" |
| **Expiración de token**            | Limitar ventana de ataque    | 15 minutos máximo                                            |
| **Token de un solo uso**           | Prevenir reutilización       | Campo `Used = true` después de usar                          |
| **Hash de contraseña**             | Proteger contraseñas         | BCrypt con work factor 10+                                   |
| **Longitud de código**             | Balance seguridad/usabilidad | 6 dígitos (1 millón de combinaciones)                        |

### Implementación Recomendada

| Aspecto                      | Descripción            | Implementación                               |
| ---------------------------- | ---------------------- | -------------------------------------------- |
| **Rate Limiting**            | Prevenir brute force   | Máx 3 intentos por email/hora                |
| **Logging**                  | Auditoría de seguridad | Registrar intentos (sin datos sensibles)     |
| **Invalidar tokens previos** | Un solo token activo   | Marcar anteriores como usados al crear nuevo |
| **HTTPS**                    | Cifrado en tránsito    | Obligatorio en producción                    |
| **CAPTCHA**                  | Prevenir bots          | reCAPTCHA v3 en forgot-password              |

### Ejemplo de Rate Limiting

```csharp
// Usar AspNetCoreRateLimit o implementación custom
// En Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/forgot-password",
            Period = "1h",
            Limit = 3
        }
    };
});
```

---

## 📁 Archivos a Crear/Modificar

### Estructura del Backend

```
📁 HoneyBalance.API/
│
├── 📁 Models/
│   ├── Usuario.cs                      (existente)
│   └── PasswordResetToken.cs           ← CREAR
│
├── 📁 DTOs/
│   ├── AuthDTOs.cs                     (existente - modificar)
│   │   ├── ForgotPasswordRequest       ← AGREGAR
│   │   └── ResetPasswordRequest        ← AGREGAR
│   └── ...
│
├── 📁 Services/
│   ├── IEmailService.cs                ← CREAR
│   └── EmailService.cs                 ← CREAR
│
├── 📁 Controllers/
│   └── AuthController.cs               ← MODIFICAR
│       ├── ForgotPassword()            ← AGREGAR
│       └── ResetPassword()             ← AGREGAR
│
├── 📁 Data/
│   └── ApplicationDbContext.cs         ← MODIFICAR
│       └── DbSet<PasswordResetToken>   ← AGREGAR
│
├── 📁 Migrations/
│   └── XXXXXX_AddPasswordResetTokens.cs ← GENERADO
│
├── appsettings.json                    ← MODIFICAR
│   └── SendGrid config                 ← AGREGAR
│
└── Program.cs                          ← MODIFICAR
    └── AddScoped<IEmailService>        ← AGREGAR
```

---

## 💻 Código Completo del Backend

### AuthController.cs - Endpoints Completos

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoneyBalance.API.Data;
using HoneyBalance.API.DTOs;
using HoneyBalance.API.Models;
using HoneyBalance.API.Services;

namespace HoneyBalance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // ... otros métodos existentes (Login, Register, etc.) ...

        /// <summary>
        /// Solicita un código de recuperación de contraseña
        /// </summary>
        /// <param name="request">Email del usuario</param>
        /// <returns>Mensaje genérico (no revela si el email existe)</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Respuesta genérica por seguridad
            var genericResponse = new {
                mensaje = "Si el email está registrado, recibirás un código de recuperación en tu correo."
            };

            try
            {
                // 1. Buscar usuario por email (case insensitive)
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                // 2. Si no existe, retornar respuesta genérica (seguridad)
                if (usuario == null)
                {
                    _logger.LogInformation("Intento de recuperación para email no registrado: {Email}", request.Email);
                    return Ok(genericResponse);
                }

                // 3. Invalidar tokens anteriores del usuario
                var tokensAnteriores = await _context.PasswordResetTokens
                    .Where(t => t.UsuarioId == usuario.UsuarioId && !t.Used)
                    .ToListAsync();

                foreach (var tokenAnterior in tokensAnteriores)
                {
                    tokenAnterior.Used = true;
                }

                // 4. Generar nuevo código de 6 dígitos
                var random = new Random();
                var token = random.Next(100000, 999999).ToString();

                // 5. Crear registro de token
                var resetToken = new PasswordResetToken
                {
                    UsuarioId = usuario.UsuarioId,
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    Used = false
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                // 6. Enviar email con el código
                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    usuario.Email,
                    usuario.NombreCompleto ?? usuario.Email,
                    token
                );

                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar email de recuperación a {Email}", usuario.Email);
                }

                _logger.LogInformation("Código de recuperación generado para usuario {UserId}", usuario.UsuarioId);
                return Ok(genericResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ForgotPassword para {Email}", request.Email);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Restablece la contraseña usando el código de verificación
        /// </summary>
        /// <param name="request">Token y nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // 1. Buscar token válido (no usado y no expirado)
                var resetToken = await _context.PasswordResetTokens
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t =>
                        t.Token == request.Token &&
                        !t.Used &&
                        t.ExpiresAt > DateTime.UtcNow);

                // 2. Validar token
                if (resetToken == null)
                {
                    _logger.LogWarning("Intento de reset con token inválido o expirado: {Token}", request.Token);
                    return BadRequest(new { mensaje = "El código es inválido o ha expirado. Solicita uno nuevo." });
                }

                // 3. Validar que el usuario existe
                if (resetToken.Usuario == null)
                {
                    return BadRequest(new { mensaje = "Usuario no encontrado" });
                }

                // 4. Hash de la nueva contraseña con BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);

                // 5. Actualizar contraseña del usuario
                resetToken.Usuario.PasswordHash = hashedPassword;

                // 6. Marcar token como usado
                resetToken.Used = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña restablecida exitosamente para usuario {UserId}", resetToken.UsuarioId);

                return Ok(new { mensaje = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ResetPassword");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
```

---

## 🧪 Pruebas con Postman/Thunder Client

### Test 1: Forgot Password (Email válido)

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
    "email": "usuario@ejemplo.com"
}
```

**Respuesta esperada (200 OK):**

```json
{
  "mensaje": "Si el email está registrado, recibirás un código de recuperación en tu correo."
}
```

### Test 2: Forgot Password (Email inválido)

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
    "email": "noexiste@ejemplo.com"
}
```

**Respuesta esperada (200 OK):** (misma respuesta por seguridad)

```json
{
  "mensaje": "Si el email está registrado, recibirás un código de recuperación en tu correo."
}
```

### Test 3: Reset Password (Token válido)

```http
POST /api/auth/reset-password
Content-Type: application/json

{
    "token": "123456",
    "newPassword": "NuevaContraseña123!"
}
```

**Respuesta esperada (200 OK):**

```json
{
  "mensaje": "Contraseña restablecida exitosamente. Ya puedes iniciar sesión."
}
```

### Test 4: Reset Password (Token inválido/expirado)

```http
POST /api/auth/reset-password
Content-Type: application/json

{
    "token": "000000",
    "newPassword": "NuevaContraseña123!"
}
```

**Respuesta esperada (400 Bad Request):**

```json
{
  "mensaje": "El código es inválido o ha expirado. Solicita uno nuevo."
}
```

---

## 📊 Endpoints del Frontend (Ya implementados)

### AuthService (Angular)

```typescript
// Ubicación: src/app/core/services/auth.service.ts

forgotPassword(email: string): Observable<{ mensaje: string }> {
  return this.http.post<{ mensaje: string }>(
    `${this.baseUrl}/forgot-password`,
    { email }
  );
}

resetPassword(token: string, newPassword: string): Observable<{ mensaje: string }> {
  return this.http.post<{ mensaje: string }>(
    `${this.baseUrl}/reset-password`,
    { token, newPassword }
  );
}
```

---

## ✅ Checklist Final

### Pre-implementación

- [ ] Cuenta de SendGrid creada y verificada
- [ ] API Key de SendGrid obtenida
- [ ] Dominio/email verificado en SendGrid

### Implementación

- [ ] Modelo `PasswordResetToken` creado
- [ ] Migración ejecutada
- [ ] `IEmailService` e implementación creados
- [ ] Servicio registrado en DI container
- [ ] DTOs creados
- [ ] Endpoint `forgot-password` implementado
- [ ] Endpoint `reset-password` implementado

### Testing

- [ ] Prueba de `forgot-password` con email válido
- [ ] Verificar recepción de email
- [ ] Prueba de `reset-password` con código válido
- [ ] Prueba de código expirado
- [ ] Prueba de código ya usado
- [ ] Flujo completo desde Angular

### Seguridad

- [ ] API Key en variables de entorno (no en código)
- [ ] Rate limiting configurado
- [ ] Logs sin datos sensibles
- [ ] HTTPS en producción

---

## 🆘 Troubleshooting

### El email no llega

1. Verificar API Key de SendGrid
2. Revisar logs del `EmailService`
3. Verificar carpeta de spam
4. Confirmar que el dominio/email está verificado en SendGrid

### Token siempre inválido

1. Verificar que la hora del servidor es correcta (UTC)
2. Confirmar que el token no se está modificando en tránsito
3. Revisar que `ExpiresAt` se está calculando correctamente

### Error de CORS

```csharp
// En Program.cs, asegurar que CORS permite el origen del frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://tudominio.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## 📞 Soporte

Si tienes dudas durante la implementación, los puntos clave a revisar son:

1. **Logs del backend** - Para errores en endpoints
2. **Network tab del navegador** - Para ver requests/responses
3. **SendGrid Activity** - Para ver estado de emails enviados
4. **Base de datos** - Tabla `PasswordResetTokens` para ver tokens generados

---

_Documento generado por GitHub Copilot para el proyecto HoneyBalance_
