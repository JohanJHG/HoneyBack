using HoneyBack.DTOs;
using HoneyBack.Servicios;
using HoneyBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoneyBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;

        public OnboardingController(IOnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
        }

        [HttpGet("status")]
        public async Task<ActionResult<OnboardingStatusDto>> ObtenerEstado()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            var estado = await _onboardingService.ObtenerEstadoAsync(userId.Value);
            return Ok(estado);
        }

        [HttpPatch("reopen")]
        public async Task<ActionResult> Reabrir()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            await _onboardingService.ReabrirAsync(userId.Value);
            return NoContent();
        }

        [HttpPatch("dismiss")]
        public async Task<ActionResult> Descartar()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado" });

            await _onboardingService.DescartarAsync(userId.Value);
            return NoContent();
        }
    }
}
