using HoneyBack.Middleware;
using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Resend;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var rawConnectionString = builder.Configuration.GetConnectionString("conexion");
if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:conexion no configurado");
}
var connectionString = NormalizeNpgsqlConnectionString(rawConnectionString);

builder.Services.AddDbContext<HoneyBalanceDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null)));

// Health checks (para Docker healthcheck y monitoreo)
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(30));

// Registrar servicios
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<ISesionesService, SesionesService>();
builder.Services.AddScoped<IMensajesContactoService, MensajesContactoService>();
builder.Services.AddScoped<IMetasAhorroService, MetasAhorroService>();
builder.Services.AddScoped<IConfiguracionesUsuarioService, ConfiguracionesUsuarioService>();
builder.Services.AddScoped<ITransaccionesService, TransaccionesService>();
builder.Services.AddScoped<IEntornosPersonalesService, EntornosPersonalesService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
// JWT Token service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
// reCAPTCHA validation service
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();

// Configurar Resend para env�o de emails
var resendApiKey = builder.Configuration["Resend:ApiKey"];
if (!string.IsNullOrWhiteSpace(resendApiKey))
{
    builder.Services.AddOptions();
    builder.Services.AddHttpClient<IResend, ResendClient>();
    builder.Services.Configure<ResendClientOptions>(options =>
    {
        options.ApiToken = resendApiKey;
    });
    builder.Services.AddScoped<IEmailService, ResendEmailService>();
}
else
{
    // Fallback: servicio que no env�a emails (solo logging)
    builder.Services.AddScoped<IEmailService, NoOpEmailService>();
    Console.WriteLine("[ADVERTENCIA] Resend:ApiKey no configurada. Los emails no se enviar�n. Configure con: dotnet user-secrets set \"Resend:ApiKey\" \"<tu-api-key>\"");
}

// Configurar CORS para Angular (desarrollo y producci�n)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();
        var csvOrigins = builder.Configuration["Cors:AllowedOriginsCsv"];

        if (!string.IsNullOrWhiteSpace(csvOrigins))
        {
            allowedOrigins = csvOrigins
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        if (allowedOrigins.Length == 0)
        {
            allowedOrigins = new[] { "http://localhost:4200", "http://localhost:4201" };
        }
        
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
              .AllowCredentials();
    });
});

// Autenticaci�n JWT
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var key = builder.Configuration["Jwt:Key"]; // usar user-secrets/variables entorno

// Validar / proporcionar clave en desarrollo para evitar excepci�n
if (string.IsNullOrWhiteSpace(key))
{
    if (builder.Environment.IsDevelopment())
    {
        // Clave solo para desarrollo. Sustituir por user-secret o variable entorno.
        key = "DEV-PLACEHOLDER-CHANGE-ME-123456789012345678901234"; // >=32 chars
        Console.WriteLine("[ADVERTENCIA] Jwt:Key no configurado. Usando clave temporal en desarrollo. Configure user-secrets (dotnet user-secrets set \"Jwt:Key\" <clave>) o variable entorno Jwt__Key.");
    }
    else
    {
        throw new InvalidOperationException("Jwt:Key no configurado. Defina user-secrets (dotnet user-secrets set \"Jwt:Key\" <clave>) o variable entorno Jwt__Key.");
    }
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole(HoneyBack.Models.RolUsuario.SuperAdmin.ToString()));
    options.AddPolicy("AdminOrSuperAdmin", policy =>
        policy.RequireRole(
            HoneyBack.Models.RolUsuario.SuperAdmin.ToString(),
            HoneyBack.Models.RolUsuario.Administrador.ToString()));
});

// Rate limiting: 5 intentos / 5 min por IP en auth, 100 req / min en API general
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        var origin = ctx.HttpContext.Request.Headers.Origin.ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            ctx.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = origin;
            ctx.HttpContext.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        }
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { mensaje = "Demasiadas solicitudes. Intenta de nuevo más tarde." });
    };

    options.AddFixedWindowLimiter("auth", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(5);
        cfg.QueueLimit = 0;
        cfg.AutoReplenishment = true;
    });

    options.AddFixedWindowLimiter("api", cfg =>
    {
        cfg.PermitLimit = 100;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueLimit = 0;
        cfg.AutoReplenishment = true;
    });
});

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "HoneyBack API", 
        Version = "v1",
        Description = "API de HoneyBack con autenticaci�n JWT"
    });
    
    // Definir el esquema de seguridad Bearer JWT
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de un espacio y luego su token JWT.\n\nEjemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    
    // Requerir el esquema de seguridad globalmente
    var securityRequirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

var applyMigrationsAtStartup = builder.Configuration.GetValue<bool?>("Database:ApplyMigrationsAtStartup")
    ?? !app.Environment.IsProduction();

var swaggerEnabled = builder.Configuration.GetValue<bool?>("Swagger:Enabled")
    ?? !app.Environment.IsProduction();

// ============================================================
// APLICAR MIGRACIONES AUTOM�TICAMENTE (cr�tico para Docker)
// ============================================================
if (applyMigrationsAtStartup)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<HoneyBalanceDbContext>();
        
        // Esperar a que la base de datos est� disponible (importante en Docker)
        var maxRetries = 15;
        var delay = TimeSpan.FromSeconds(5);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                logger.LogInformation("Intentando conectar a la base de datos (intento {Attempt}/{MaxRetries})...", i + 1, maxRetries);
                
                if (await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("? Conexi�n a la base de datos establecida exitosamente.");
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "? No se pudo conectar a la base de datos. Reintentando en {Delay} segundos... (Intento {Attempt}/{MaxRetries})", delay.TotalSeconds, i + 1, maxRetries);
                
                if (i == maxRetries - 1)
                {
                    logger.LogError("? No se pudo establecer conexi�n con la base de datos despu�s de {MaxRetries} intentos.", maxRetries);
                    throw;
                }
                
                await Task.Delay(delay);
            }
        }
        
        // Aplicar migraciones pendientes
        logger.LogInformation("Verificando y aplicando migraciones de base de datos...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Se encontraron {Count} migraciones pendientes. Aplicando...", pendingMigrations.Count());
            await context.Database.MigrateAsync();
            logger.LogInformation("? Migraciones aplicadas exitosamente.");
        }
        else
        {
            logger.LogInformation("? La base de datos est� actualizada. No hay migraciones pendientes.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Error cr�tico al aplicar migraciones de base de datos.");
        // En desarrollo, podemos continuar; en producci�n, deber�a fallar
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
        logger.LogWarning("Continuando en modo desarrollo a pesar del error de migraci�n.");
    }
}
else
{
    app.Logger.LogInformation("Database:ApplyMigrationsAtStartup=false. Se omite la ejecuci�n de migraciones autom�ticas.");
}

// Seguridad: primero los middlewares transversales
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Habilitar CORS
app.UseCors("AllowAngularApp");

// Rate limiting antes de autenticación
app.UseRateLimiter();

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HoneyBack API v1");
        c.RoutePrefix = "swagger";
    });
}

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string NormalizeNpgsqlConnectionString(string inputConnectionString)
{
    var sanitized = inputConnectionString.Trim().Trim('"').Trim();

    if (sanitized.StartsWith("<", StringComparison.Ordinal) && sanitized.EndsWith(">", StringComparison.Ordinal))
    {
        sanitized = sanitized[1..^1].Trim();
    }

    // Strip SQLAlchemy driver qualifier (e.g. +asyncpg) so postgresql+asyncpg:// → postgresql://
    if (sanitized.StartsWith("postgresql+", StringComparison.OrdinalIgnoreCase))
    {
        var schemeEnd = sanitized.IndexOf("://");
        if (schemeEnd > 0)
            sanitized = "postgresql" + sanitized[schemeEnd..];
    }

    var isPostgresUri =
        sanitized.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        sanitized.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase);

    if (!isPostgresUri)
    {
        return sanitized;
    }

    var uri = new Uri(sanitized, UriKind.Absolute);
    var userInfo = uri.UserInfo.Split(':', 2);
    var database = uri.AbsolutePath.Trim('/');

    if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(database))
    {
        throw new InvalidOperationException("ConnectionStrings:conexion no tiene formato válido para PostgreSQL URI.");
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = database,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };

    var queryParams = uri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries);

    foreach (var pair in queryParams)
    {
        var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
        var key = Uri.UnescapeDataString(parts[0]);
        var value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

        if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase) &&
            Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode))
        {
            builder.SslMode = sslMode;
            continue;
        }

        if ((key.Equals("channel_binding", StringComparison.OrdinalIgnoreCase) ||
             key.Equals("channelbinding", StringComparison.OrdinalIgnoreCase)) &&
            Enum.TryParse<ChannelBinding>(value, ignoreCase: true, out var channelBinding))
        {
            builder.ChannelBinding = channelBinding;
        }
    }

    return builder.ConnectionString;
}
