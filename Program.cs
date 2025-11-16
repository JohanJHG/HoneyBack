using HoneyBack.Models;
using HoneyBack.Servicios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configuracion de entity framework con SQL
builder.Services.AddDbContext<HoneyBalanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

// Registrar servicios
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<ISesionesService, SesionesService>();
builder.Services.AddScoped<IMensajesContactoService, MensajesContactoService>();

// CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201") // URL típica de Angular en desarrollo
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Habilitar CORS
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
