using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Data.Seeders
{
    public class AnalyticsSeedService(HoneyBalanceDbContext context, ILogger<AnalyticsSeedService> logger)
    {
        private static readonly string[] Nombres =
        [
            "Carlos", "María", "Juan", "Ana", "Luis", "Laura", "Jorge", "Paula", "Diego", "Sofía",
            "Andrés", "Valentina", "Sebastián", "Camila", "Felipe", "Isabella", "Nicolás", "Juliana",
            "Alejandro", "Daniela", "Roberto", "Natalia", "Miguel", "Carolina", "Tomás", "Gabriela",
            "Ricardo", "Mariana", "Esteban", "Lucía"
        ];

        private static readonly string[] Apellidos =
        [
            "García", "Martínez", "López", "González", "Rodríguez", "Pérez", "Sánchez", "Ramírez",
            "Torres", "Flores", "Rivera", "Gómez", "Díaz", "Reyes", "Cruz", "Morales", "Ortiz",
            "Vargas", "Castillo", "Jiménez", "Moreno", "Medina", "Ruiz", "Herrera", "Aguilar",
            "Gutiérrez", "Mendoza", "Rojas", "Cárdenas", "Ospina"
        ];

        private static readonly (string Nombre, string Categoria, decimal Min, decimal Max)[] GastosCatalogo =
        [
            ("Mercado Éxito", "alimentación", 50_000, 350_000),
            ("Rappi - Comida", "alimentación", 20_000, 80_000),
            ("D1 supermercado", "alimentación", 30_000, 150_000),
            ("Restaurante Crepes & Waffles", "alimentación", 40_000, 120_000),
            ("Uber", "transporte", 8_000, 35_000),
            ("InDriver", "transporte", 6_000, 30_000),
            ("SITP - Recarga", "transporte", 10_000, 50_000),
            ("Gasolina", "transporte", 80_000, 250_000),
            ("Netflix", "entretenimiento", 18_900, 18_900),
            ("Spotify", "entretenimiento", 16_900, 16_900),
            ("Disney+ Colombia", "entretenimiento", 21_900, 21_900),
            ("Cine Colombia", "entretenimiento", 15_000, 40_000),
            ("Consulta médica EPS", "salud", 8_800, 30_000),
            ("Farmacia Cruz Verde", "salud", 15_000, 120_000),
            ("Gimnasio SmartFit", "salud", 60_000, 90_000),
            ("Odontólogo", "salud", 80_000, 400_000),
            ("Universidad Nacional", "educación", 500_000, 2_000_000),
            ("Curso Platzi", "educación", 50_000, 300_000),
            ("Libros Panamericana", "educación", 30_000, 100_000),
            ("Arriendo apartamento", "hogar", 800_000, 2_500_000),
            ("Claro internet hogar", "hogar", 60_000, 120_000),
            ("Gas Natural Fenosa", "hogar", 40_000, 120_000),
            ("EPM energía", "hogar", 50_000, 200_000),
            ("Zara", "ropa", 80_000, 400_000),
            ("Adidas Colombia", "ropa", 100_000, 500_000),
            ("C&A", "ropa", 40_000, 200_000),
            ("Apple Store", "tecnología", 100_000, 800_000),
            ("Samsung tienda", "tecnología", 80_000, 600_000),
            ("Alkosto electrónica", "tecnología", 50_000, 400_000),
            ("GymPass", "deporte", 40_000, 100_000),
            ("Decathlon", "deporte", 50_000, 300_000),
            ("Peluquería", "belleza", 25_000, 80_000),
            ("Salón de belleza", "belleza", 40_000, 150_000),
        ];

        private static readonly (string Nombre, string Categoria, decimal Min, decimal Max)[] IngresosCatalogo =
        [
            ("Salario mensual", "salario", 1_500_000, 8_000_000),
            ("Pago freelance", "freelance", 200_000, 2_000_000),
            ("Honorarios proyecto", "freelance", 500_000, 3_000_000),
            ("Dividendos acciones", "inversiones", 50_000, 500_000),
            ("Transferencia familiar", "transferencia", 100_000, 1_000_000),
        ];

        private static readonly (string Nombre, string Categoria, string Color, string Icono)[] MetasCatalogo =
        [
            ("Fondo de emergencia", "emergencia", "#EF4444", "shield"),
            ("Vacaciones Cartagena", "vacaciones", "#3B82F6", "plane"),
            ("Cuota inicial apartamento", "vivienda", "#10B981", "home"),
            ("Moto Honda CB190", "vehiculo", "#F59E0B", "bike"),
            ("Maestría en finanzas", "educacion", "#8B5CF6", "book"),
            ("MacBook Pro", "tecnologia", "#6B7280", "laptop"),
            ("Fondo ahorro general", "ahorro", "#FFD8A9", "piggy-bank"),
            ("Viaje a Europa", "vacaciones", "#EC4899", "globe"),
            ("Carro familiar", "vehiculo", "#14B8A6", "car"),
            ("Renovación hogar", "vivienda", "#F97316", "wrench"),
            ("Inversión inicial", "ahorro", "#22C55E", "chart"),
            ("Posgrado universidad", "educacion", "#A855F7", "graduation-cap"),
        ];

        private static readonly string[] IpsColombianas =
        [
            "181.58.30.12", "190.25.146.88", "181.143.207.45", "190.248.12.67",
            "186.31.45.123", "200.69.102.4", "181.78.98.201", "190.26.150.30",
        ];

        private static readonly string[] AsuntosMensaje =
        [
            "Problema con mi cuenta", "Consulta sobre planes", "Error al registrar gasto",
            "No puedo iniciar sesión", "Solicitud de eliminación de cuenta", "Duda sobre metas de ahorro",
            "Cómo exportar mis datos", "Aplicación muy lenta", "Quiero reportar un bug",
            "Sugerencia de mejora", "Problema con la sincronización", "Error en el dashboard",
        ];

        private static readonly string[] RespuestasBase =
        [
            "Hemos recibido tu mensaje y lo estamos procesando. Te contactaremos pronto.",
            "Gracias por reportar este problema. Ya fue escalado a nuestro equipo técnico.",
            "Hemos solucionado el problema. Por favor, intenta nuevamente.",
            "Lamentamos los inconvenientes. El equipo está trabajando en ello.",
        ];

        public async Task SeedAsync()
        {
            if (await context.Usuarios.AnyAsync(u => u.Email.EndsWith("@honeybalance.test")))
            {
                logger.LogInformation("Seed ya ejecutado. Omitiendo.");
                return;
            }

            logger.LogInformation("Iniciando seed de datos analytics...");

            // ponytail: CreateExecutionStrategy requerido cuando EnableRetryOnFailure está activo
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                var rng = new Random(42);
                var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

                // ponytail: un solo hash BCrypt para los 120 usuarios seed
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Seed1234!");

                // ─── 1. USUARIOS ───────────────────────────────────────────────
                var usuarios = new List<Usuario>(120);
                for (int i = 1; i <= 120; i++)
                {
                    var nombre = Nombres[rng.Next(Nombres.Length)];
                    var apellido1 = Apellidos[rng.Next(Apellidos.Length)];
                    var apellido2 = Apellidos[rng.Next(Apellidos.Length)];

                    // Distribución: 70 veteranos, 30 medios, 20 recientes. 10 inactivos.
                    int diasAtras = i switch
                    {
                        <= 70 => rng.Next(90, 365),
                        <= 100 => rng.Next(30, 90),
                        _ => rng.Next(1, 30)
                    };
                    bool activo = i > 110 ? false : true; // últimos 10 = inactivos (churn)

                    string moneda = rng.Next(10) < 9 ? "COP" : (rng.Next(2) == 0 ? "USD" : "EUR");

                    usuarios.Add(new Usuario
                    {
                        NombreCompleto = $"{nombre} {apellido1} {apellido2}",
                        Email = $"seed_user_{i:D3}@honeybalance.test",
                        PasswordHash = passwordHash,
                        FechaRegistro = DateTime.SpecifyKind(ahora.AddDays(-diasAtras), DateTimeKind.Unspecified),
                        Activo = activo,
                        PreferenciasMoneda = moneda,
                        Rol = RolUsuario.Usuario,
                    });
                }
                context.Usuarios.AddRange(usuarios);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} usuarios creados", usuarios.Count);

                // ─── 2. CONFIGURACIONES ────────────────────────────────────────
                var configs = usuarios.Select(u =>
                {
                    var diasRegistrado = (ahora - u.FechaRegistro).TotalDays;
                    return new ConfiguracionesUsuario
                    {
                        UsuarioId = u.UsuarioId,
                        Tema = rng.Next(10) < 7 ? "dark" : "light",
                        Idioma = "es",
                        Timezone = "America/Bogota",
                        FormatoFecha = "DD/MM/YYYY",
                        MonedaPreferida = u.PreferenciasMoneda,
                        NotificacionesEmail = rng.Next(2) == 0,
                        NotificacionesPush = rng.Next(2) == 0,
                        PrimeraVez = diasRegistrado < 30,
                        EsVeterano = diasRegistrado > 180,
                        FechaCreacion = DateTime.SpecifyKind(u.FechaRegistro, DateTimeKind.Unspecified),
                        FechaActualizacion = DateTime.SpecifyKind(u.FechaRegistro.AddDays(rng.Next(1, 10)), DateTimeKind.Unspecified),
                    };
                }).ToList();
                context.ConfiguracionesUsuarios.AddRange(configs);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} configuraciones creadas", configs.Count);

                // ─── 3. ONBOARDING ─────────────────────────────────────────────
                var onboardings = usuarios.Select(u =>
                {
                    var diasRegistrado = (ahora - u.FechaRegistro).TotalDays;
                    bool dismissed = diasRegistrado > 30;
                    return new OnboardingUsuario
                    {
                        UsuarioId = u.UsuarioId,
                        Dismissed = dismissed,
                        DismissedAt = dismissed
                            ? DateTime.SpecifyKind(u.FechaRegistro.AddDays(rng.Next(1, 8)), DateTimeKind.Unspecified)
                            : null,
                        FechaCreacion = DateTime.SpecifyKind(u.FechaRegistro, DateTimeKind.Unspecified),
                        FechaActualizacion = DateTime.SpecifyKind(u.FechaRegistro, DateTimeKind.Unspecified),
                    };
                }).ToList();
                context.OnboardingUsuarios.AddRange(onboardings);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} onboardings creados", onboardings.Count);

                // ─── 4. METAS DE AHORRO ────────────────────────────────────────
                var metas = new List<MetasAhorro>();
                foreach (var u in usuarios)
                {
                    int numMetas = rng.Next(1, 5);
                    for (int m = 0; m < numMetas; m++)
                    {
                        var plantilla = MetasCatalogo[rng.Next(MetasCatalogo.Length)];
                        var montoObjetivo = Math.Round((decimal)(rng.NextDouble() * 14_500_000 + 500_000), 2);
                        var diasAtras = rng.Next(30, 365);
                        var fechaInicio = hoy.AddDays(-diasAtras);
                        var fechaObjetivo = fechaInicio.AddMonths(rng.Next(1, 25));

                        // 50% en progreso, 30% completadas, 20% abandonadas
                        int estado = rng.Next(10);
                        bool completada = estado >= 8;
                        bool activa = estado < 5;
                        var montoActual = completada
                            ? montoObjetivo
                            : Math.Round(montoObjetivo * (decimal)rng.NextDouble(), 2);

                        metas.Add(new MetasAhorro
                        {
                            UsuarioId = u.UsuarioId,
                            Nombre = plantilla.Nombre,
                            Categoria = plantilla.Categoria,
                            Color = plantilla.Color,
                            Icono = plantilla.Icono,
                            MontoObjetivo = montoObjetivo,
                            MontoActual = montoActual,
                            FechaInicio = fechaInicio,
                            FechaObjetivo = fechaObjetivo,
                            FechaCompletada = completada
                                ? DateTime.SpecifyKind(ahora.AddDays(-rng.Next(1, diasAtras)), DateTimeKind.Unspecified)
                                : null,
                            Activa = activa,
                            Completada = completada,
                            Prioridad = rng.Next(0, 4),
                        });
                    }
                }
                context.MetasAhorros.AddRange(metas);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} metas creadas", metas.Count);

                // ─── 5. TRANSACCIONES ──────────────────────────────────────────
                // Pesos por mes: simula estacionalidad (dic y ene tienen más gastos)
                int[] pesosMes = [3, 2, 3, 4, 4, 5, 4, 4, 4, 5, 4, 6];

                var transacciones = new List<Transaccione>();
                foreach (var u in usuarios)
                {
                    int numTx = rng.Next(20, 61);
                    for (int t = 0; t < numTx; t++)
                    {
                        // Seleccionar mes proporcional al peso
                        int mesAtras = ElegirMesPonderado(rng, pesosMes);
                        var fecha = hoy.AddMonths(-mesAtras).AddDays(rng.Next(0, 28));

                        bool esGasto = rng.Next(4) < 3; // 75% gastos
                        if (esGasto)
                        {
                            var cat = GastosCatalogo[rng.Next(GastosCatalogo.Length)];
                            transacciones.Add(new Transaccione
                            {
                                UsuarioId = u.UsuarioId,
                                Nombre = cat.Nombre,
                                Tipo = "gasto",
                                Categoria = cat.Categoria,
                                Monto = Math.Round(cat.Min + (decimal)rng.NextDouble() * (cat.Max - cat.Min), 0),
                                Fecha = fecha,
                                FechaCreacion = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified),
                            });
                        }
                        else
                        {
                            var cat = IngresosCatalogo[rng.Next(IngresosCatalogo.Length)];
                            transacciones.Add(new Transaccione
                            {
                                UsuarioId = u.UsuarioId,
                                Nombre = cat.Nombre,
                                Tipo = "ingreso",
                                Categoria = cat.Categoria,
                                Monto = Math.Round(cat.Min + (decimal)rng.NextDouble() * (cat.Max - cat.Min), 0),
                                Fecha = fecha,
                                FechaCreacion = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified),
                            });
                        }
                    }
                }
                context.Transacciones.AddRange(transacciones);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} transacciones creadas", transacciones.Count);

                // ─── 6. ENTORNOS PERSONALES ────────────────────────────────────
                string[] modulosClave = ["presupuesto", "gastos", "ahorro", "dashboard", "entorno_personal"];
                string[] titulosModulo = ["Mi Presupuesto", "Control de Gastos", "Plan de Ahorro", "Panel Principal", "Entorno Personal"];

                var entornos = new List<EntornoPersonal>();
                foreach (var u in usuarios)
                {
                    int numEntornos = rng.Next(1, 4);
                    var indicesTomados = new HashSet<int>();
                    for (int e = 0; e < numEntornos; e++)
                    {
                        int idx;
                        do { idx = rng.Next(modulosClave.Length); } while (!indicesTomados.Add(idx));

                        var fechaCreacion = DateTime.SpecifyKind(u.FechaRegistro.AddDays(rng.Next(1, 15)), DateTimeKind.Unspecified);
                        entornos.Add(new EntornoPersonal
                        {
                            UsuarioId = u.UsuarioId,
                            ModuloClave = modulosClave[idx],
                            Titulo = titulosModulo[idx],
                            Subtitulo = $"Configuración personalizada de {titulosModulo[idx].ToLower()}",
                            ValorPrincipal = rng.Next(1_000_000, 10_000_000).ToString(),
                            Etiqueta = rng.Next(2) == 0 ? "Activo" : "Configurado",
                            DatosJson = $"{{\"version\":1,\"modulo\":\"{modulosClave[idx]}\",\"config\":{{\"activo\":true}}}}",
                            FechaCreacion = fechaCreacion,
                            FechaActualizacion = fechaCreacion,
                        });
                    }
                }
                context.EntornosPersonales.AddRange(entornos);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} entornos personales creados", entornos.Count);

                // ─── 7. SESIONES (para AVG session analytics) ─────────────────
                // Duraciones realistas: normal centrada en ~18 min (5-45 min)
                var sesiones = new List<Sesione>();
                foreach (var u in usuarios)
                {
                    int numSesiones = rng.Next(3, 9);
                    for (int s = 0; s < numSesiones; s++)
                    {
                        var diasAtras = rng.Next(0, 30);
                        var horasAtras = rng.Next(0, 24);
                        var inicio = DateTime.SpecifyKind(ahora.AddDays(-diasAtras).AddHours(-horasAtras), DateTimeKind.Unspecified);
                        // Duración: 5-45 min con sesgo hacia 15-20 min
                        var duracionMin = 5 + (int)(Math.Abs(rng.NextDouble() + rng.NextDouble() - 1) * 40);
                        var fin = inicio.AddMinutes(duracionMin);

                        sesiones.Add(new Sesione
                        {
                            UsuarioId = u.UsuarioId,
                            TokenSesion = Guid.NewGuid().ToString(),
                            FechaCreacion = inicio,
                            FechaExpiracion = DateTime.SpecifyKind(fin, DateTimeKind.Unspecified),
                            Activa = false, // históricas → usadas por analytics
                            Ipaddress = IpsColombianas[rng.Next(IpsColombianas.Length)],
                        });
                    }
                }
                context.Sesiones.AddRange(sesiones);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} sesiones históricas creadas", sesiones.Count);

                // ─── 8. MENSAJES DE CONTACTO ───────────────────────────────────
                var usuariosConId = usuarios.Take(20).ToList();
                var mensajes = new List<MensajesContacto>();
                for (int i = 0; i < 40; i++)
                {
                    bool esRegistrado = i < 20;
                    bool leido = rng.Next(10) < 4; // 40% leídos
                    var fechaEnvio = DateTime.SpecifyKind(ahora.AddDays(-rng.Next(1, 90)), DateTimeKind.Unspecified);
                    var asunto = AsuntosMensaje[rng.Next(AsuntosMensaje.Length)];
                    Usuario? usuarioRef = esRegistrado ? usuariosConId[i] : null;

                    mensajes.Add(new MensajesContacto
                    {
                        Nombre = esRegistrado ? usuarioRef!.NombreCompleto : $"Visitante {i - 20 + 1}",
                        Email = esRegistrado ? usuarioRef!.Email : $"visitante{i - 20 + 1}@gmail.com",
                        Asunto = asunto,
                        Mensaje = $"Hola, tengo un problema relacionado con: {asunto.ToLower()}. Necesito ayuda lo antes posible.",
                        FechaEnvio = fechaEnvio,
                        UsuarioId = usuarioRef?.UsuarioId,
                        Leido = leido,
                        FechaLeido = leido ? DateTime.SpecifyKind(fechaEnvio.AddHours(rng.Next(1, 48)), DateTimeKind.Unspecified) : null,
                        Respuesta = leido ? RespuestasBase[rng.Next(RespuestasBase.Length)] : null,
                        FechaRespuesta = leido ? DateTime.SpecifyKind(fechaEnvio.AddHours(rng.Next(2, 72)), DateTimeKind.Unspecified) : null,
                    });
                }
                context.MensajesContactos.AddRange(mensajes);
                await context.SaveChangesAsync();
                logger.LogInformation("✓ {Count} mensajes de contacto creados", mensajes.Count);

                await tx.CommitAsync();
                logger.LogInformation(
                    "Seed completado: {U} usuarios, {T} transacciones, {M} metas, {E} entornos, {S} sesiones, {C} mensajes",
                    usuarios.Count, transacciones.Count, metas.Count, entornos.Count, sesiones.Count, mensajes.Count);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                logger.LogError(ex, "Error durante el seed. Rollback ejecutado.");
                throw;
            }
            }); // end ExecutionStrategy
        }

        // Selecciona un mes (0=actual, 11=hace 11 meses) con probabilidad proporcional al peso
        private static int ElegirMesPonderado(Random rng, int[] pesos)
        {
            var total = pesos.Sum();
            var punto = rng.Next(total);
            var acum = 0;
            for (int i = 0; i < pesos.Length; i++)
            {
                acum += pesos[i];
                if (punto < acum) return i;
            }
            return pesos.Length - 1;
        }
    }
}
