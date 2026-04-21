namespace Projeto.Moope.Pagamento.Api.Utils
{
    public class JwtSettings
    {
        public string Audience { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public bool UseJwks { get; set; } = true;
    }
}

