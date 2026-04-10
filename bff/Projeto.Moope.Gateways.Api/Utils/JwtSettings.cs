namespace Projeto.Moope.Gateways.Api.Utils
{
    public class JwtSettings
    {
        public bool UseJwks { get; set; }

        public string Authority { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;
        public int ExpiracaoHoras { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
