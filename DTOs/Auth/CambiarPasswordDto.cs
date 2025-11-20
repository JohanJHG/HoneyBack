namespace HoneyBack.DTOs.Auth
{
    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; } = null!;
        public string PasswordNueva { get; set; } = null!;
    }
}
