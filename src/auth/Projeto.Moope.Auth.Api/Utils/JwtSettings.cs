namespace Projeto.Moope.Auth.Api.Utils
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public int ExpiracaoHoras { get; set; }
        public int ExpiracaoRefreshTokenDias { get; set; } = 7;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
