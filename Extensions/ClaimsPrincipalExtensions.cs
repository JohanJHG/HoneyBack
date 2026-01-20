using System.Security.Claims;

namespace HoneyBack.Extensions
{
    /// <summary>
    /// Extensiones para ClaimsPrincipal que facilitan la extracción segura del UserId del token JWT
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extrae el UserId del token JWT de forma segura.
        /// Busca en los claims estándar: "UserId", "sub", NameIdentifier
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>UserId o null si no se encuentra</returns>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            // Intentar obtener de claim personalizado "UserId"
            var userIdClaim = user.FindFirst("UserId")
                ?? user.FindFirst("userId")
                ?? user.FindFirst("sub")
                ?? user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el UserId o lanza excepción si no existe
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>UserId del usuario</returns>
        /// <exception cref="UnauthorizedAccessException">Si no se puede obtener el ID del usuario</exception>
        public static int GetUserIdRequired(this ClaimsPrincipal user)
        {
            var userId = user.GetUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario del token");
            }
            return userId.Value;
        }
    }
}
