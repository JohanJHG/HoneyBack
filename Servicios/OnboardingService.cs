using HoneyBack.DTOs;
using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class OnboardingService : IOnboardingService
    {
        private readonly HoneyBalanceDbContext _context;

        public OnboardingService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<OnboardingStatusDto> ObtenerEstadoAsync(int usuarioId)
        {
            var onboarding = await _context.OnboardingUsuarios
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId);

            // Garantiza que todo usuario vea el wizard al menos una vez.
            if (onboarding is null)
            {
                var ts = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                onboarding = new OnboardingUsuario
                {
                    UsuarioId = usuarioId,
                    Dismissed = false,
                    FechaCreacion = ts,
                    FechaActualizacion = ts,
                };
                _context.OnboardingUsuarios.Add(onboarding);
                await _context.SaveChangesAsync();
            }

            var primerEntorno = await _context.EntornosPersonales
                .Where(e => e.UsuarioId == usuarioId)
                .OrderBy(e => e.FechaCreacion)
                .FirstOrDefaultAsync();

            var tieneEntorno = primerEntorno != null;
            var entornoId = primerEntorno?.EntornoId;

            var tieneTransaccion = false;
            var tieneMetaAhorro = false;

            if (tieneEntorno)
            {
                tieneTransaccion = await _context.Transacciones
                    .AnyAsync(t => t.UsuarioId == usuarioId &&
                                   t.FechaCreacion >= primerEntorno!.FechaCreacion);

                tieneMetaAhorro = await _context.MetasAhorros
                    .AnyAsync(m => m.UsuarioId == usuarioId);
            }

            var steps = new List<OnboardingStepDto>
            {
                new()
                {
                    Id = "entorno",
                    Label = "Activa tu primer entorno",
                    Description = "Elige un módulo financiero y configúralo para empezar",
                    Completed = tieneEntorno,
                    Locked = false,
                    Href = "/dashboard/entornos",
                },
                new()
                {
                    Id = "transaccion",
                    Label = "Registra tu primer movimiento",
                    Description = "Agrega un ingreso o gasto para ver tus finanzas en tiempo real",
                    Completed = tieneTransaccion,
                    Locked = !tieneEntorno,
                    Href = "/dashboard/transacciones",
                },
                new()
                {
                    Id = "meta_ahorro",
                    Label = "Crea una meta de ahorro",
                    Description = "Define un objetivo y empieza a construir tu futuro financiero",
                    Completed = tieneMetaAhorro,
                    Locked = !tieneEntorno,
                    Href = "/dashboard/ahorro",
                },
            };

            var completedCount = steps.Count(s => s.Completed);
            var currentStepIndex = steps.FindIndex(s => !s.Completed && !s.Locked);
            if (currentStepIndex == -1) currentStepIndex = completedCount >= 3 ? 3 : 0;

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (completedCount == 3 && onboarding != null && onboarding.CompletedAt == null)
            {
                onboarding.CompletedAt = ahora;
                onboarding.FechaActualizacion = ahora;
                await _context.SaveChangesAsync();
            }

            return new OnboardingStatusDto
            {
                Dismissed = onboarding?.Dismissed ?? false,
                CompletedCount = completedCount,
                TotalSteps = steps.Count,
                CurrentStep = currentStepIndex,
                EntornoId = entornoId,
                Steps = steps,
            };
        }

        public async Task ReabrirAsync(int usuarioId)
        {
            var onboarding = await _context.OnboardingUsuarios
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId);

            if (onboarding is null) return;

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            onboarding.Dismissed = false;
            onboarding.DismissedAt = null;
            onboarding.FechaActualizacion = ahora;

            await _context.SaveChangesAsync();
        }

        public async Task DescartarAsync(int usuarioId)
        {
            var onboarding = await _context.OnboardingUsuarios
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId);

            var ahora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (onboarding is null)
            {
                _context.OnboardingUsuarios.Add(new OnboardingUsuario
                {
                    UsuarioId = usuarioId,
                    Dismissed = true,
                    DismissedAt = ahora,
                    FechaCreacion = ahora,
                    FechaActualizacion = ahora,
                });
            }
            else
            {
                onboarding.Dismissed = true;
                onboarding.DismissedAt = ahora;
                onboarding.FechaActualizacion = ahora;
            }

            await _context.SaveChangesAsync();
        }
    }
}
