namespace HoneyBack.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
