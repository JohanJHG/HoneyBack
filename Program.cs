using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configurar Entity Framework con SQL Server
builder.Services.AddDbContext<HoneyBalanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

// Registrar servicios
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<ISesionesService, SesionesService>();
builder.Services.AddScoped<IMensajesContactoService, MensajesContactoService>();
builder.Services.AddScoped<ICategoriasTransaccionesService, CategoriasTransaccionesService>();
builder.Services.AddScoped<IMetasAhorroService, MetasAhorroService>();
builder.Services.AddScoped<IEstadisticasMensualesService, EstadisticasMensualesService>();
builder.Services.AddScoped<ITemplatesService, TemplatesService>();
builder.Services.AddScoped<IConfiguracionesUsuarioService, ConfiguracionesUsuarioService>();
builder.Services.AddScoped<ITransaccionesService, TransaccionesService>();
// JWT Token service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configurar Resend para envío de emails
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
    // Fallback: servicio que no envía emails (solo logging)
    builder.Services.AddScoped<IEmailService, NoOpEmailService>();
    Console.WriteLine("[ADVERTENCIA] Resend:ApiKey no configurada. Los emails no se enviaran. Configure con: dotnet user-secrets set \"Resend:ApiKey\" \"<tu-api-key>\"");
}

// Configurar CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Autenticación JWT
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var key = builder.Configuration["Jwt:Key"]; // usar user-secrets/variables entorno

// Validar / proporcionar clave en desarrollo para evitar excepción
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

builder.Services.AddAuthorization();

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "HoneyBack API", 
        Version = "v1",
        Description = "API de HoneyBack con autenticación JWT"
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

// Habilitar CORS
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
