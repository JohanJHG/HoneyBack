using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoneyBack.Models;

/// <summary>
/// Token para recuperación de contraseña.
/// Expira en 15 minutos y es de un solo uso.
/// </summary>
public class PasswordResetToken
{
    [Key]
    public int TokenId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    /// <summary>
    /// Código de 6 dígitos generado criptográficamente.
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Token { get; set; } = null!;

    /// <summary>
    /// Fecha/hora de creación en UTC.
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Fecha/hora de expiración en UTC (15 minutos después de creación).
    /// </summary>
    [Required]
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Indica si el token ya fue utilizado.
    /// </summary>
    public bool Used { get; set; } = false;

    // Navigation property
    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;
}
