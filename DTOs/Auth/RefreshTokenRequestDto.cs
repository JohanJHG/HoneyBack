using System.ComponentModel.DataAnnotations;

namespace HoneyBack.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [StringLength(200, ErrorMessage = "Token inválido")]
    public string? RefreshToken { get; set; }
}
