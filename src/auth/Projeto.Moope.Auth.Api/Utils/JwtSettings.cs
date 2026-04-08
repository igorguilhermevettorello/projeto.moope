namespace Projeto.Moope.Auth.Api.Utils
{
    public class JwtSettings
    {
        /// <summary>Segredo simétrico (HS256). Usado apenas se <see cref="RsaPrivateKeyPem"/> estiver vazio.</summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>Chave privada RSA em PEM (PKCS#8). Quando preenchida, os tokens usam RS256 e o endpoint JWKS fica disponível.</summary>
        public string RsaPrivateKeyPem { get; set; } = string.Empty;

        /// <summary>Identificador da chave ativa no cabeçalho <c>kid</c> e no documento JWKS.</summary>
        public string KeyId { get; set; } = "moope-auth-1";

        /// <summary>
        /// Chaves públicas RSA anteriores para suporte a rotação.
        /// Cada entrada expõe a chave pública antiga no JWKS, permitindo que os consumers
        /// validem tokens emitidos antes da troca de chave durante o período de transição.
        /// </summary>
        public List<PreviousRsaKey> PreviousPublicKeyPems { get; set; } = [];

        public int ExpiracaoHoras { get; set; }
        public int ExpiracaoRefreshTokenDias { get; set; } = 7;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>Representa uma chave pública RSA anterior usada durante rotação.</summary>
        public sealed class PreviousRsaKey
        {
            /// <summary>Deve corresponder ao <c>kid</c> original com que os tokens foram emitidos.</summary>
            public string KeyId { get; set; } = string.Empty;

            /// <summary>Chave pública RSA em PEM (PKCS#8 ou SPKI). Nunca use a chave privada aqui.</summary>
            public string PublicKeyPem { get; set; } = string.Empty;
        }
    }
}
