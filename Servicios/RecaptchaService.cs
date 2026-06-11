using System.Text.Json;

namespace HoneyBack.Servicios
{
    public interface IRecaptchaService
    {
        Task<bool> ValidarTokenAsync(string token);
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        public RecaptchaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RecaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogWarning("Recaptcha:SecretKey no configurado — validación omitida");
                return true;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = token
                });

                var response = await client.PostAsync(VerifyUrl, content);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token reCAPTCHA");
                return false;
            }
        }
    }
}
