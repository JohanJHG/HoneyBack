using HoneyBack.Models;

namespace HoneyBack.Servicios
{
    public interface IJwtTokenService
    {
        (string token, DateTime expiresAt) Generate(Usuario user);
    }
}
