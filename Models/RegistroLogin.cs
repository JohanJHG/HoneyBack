namespace HoneyBack.Models;

public class RegistroLogin
{
    public long RegistroLoginId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaLogin { get; set; }
    public string? Ip { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
