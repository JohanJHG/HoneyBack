using HoneyBack.DTOs;

namespace HoneyBack.Servicios
{
    public interface IOnboardingService
    {
        Task<OnboardingStatusDto> ObtenerEstadoAsync(int usuarioId);
        Task DescartarAsync(int usuarioId);
        Task ReabrirAsync(int usuarioId);
    }
}
