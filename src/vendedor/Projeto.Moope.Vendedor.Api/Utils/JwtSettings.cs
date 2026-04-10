namespace Projeto.Moope.Vendedor.Api.Utils
{
    public class JwtSettings
    {
        /// <summary>Quando true, valida o JWT via metadados OpenID (JWKS) em <see cref="Authority"/>.</summary>
        public bool UseJwks { get; set; }

        /// <summary>URL base do serviço de autenticação (igual ao <c>iss</c> do token). Ex.: https://auth.seudominio.com</summary>
        public string Authority { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;
        public int ExpiracaoHoras { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
