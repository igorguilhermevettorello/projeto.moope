using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Auth.Api.Utils;
using System.Security.Cryptography;
using System.Text;

namespace Projeto.Moope.Auth.Api.Services
{
    public sealed class JwtSigningKeyProvider : IJwtSigningKeyProvider
    {
        private readonly Lazy<MaterializedKeys> _materialized;

        public JwtSigningKeyProvider(IOptions<JwtSettings> jwtOptions)
        {
            _materialized = new Lazy<MaterializedKeys>(() => Materialize(jwtOptions.Value));
        }

        public bool UsesRsa => _materialized.Value.UsesRsa;

        public SigningCredentials GetSigningCredentials() => _materialized.Value.SigningCredentials;

        public SecurityKey GetIssuerSigningKey() => _materialized.Value.IssuerSigningKey;

        public string GetJwksJson() => _materialized.Value.JwksJson;

        private static MaterializedKeys Materialize(JwtSettings settings)
        {
            var pem = settings.RsaPrivateKeyPem?.Trim();
            if (!string.IsNullOrEmpty(pem))
            {
                pem = pem.Replace("\\n", "\n", StringComparison.Ordinal);
                using var rsa = RSA.Create();
                rsa.ImportFromPem(pem);
                var kid = string.IsNullOrWhiteSpace(settings.KeyId) ? "moope-auth-1" : settings.KeyId;
                var fullKey = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = kid };
                var signingCredentials = new SigningCredentials(fullKey, SecurityAlgorithms.RsaSha256);
                var currentPublicKey = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = kid };

                var previousKeys = LoadPreviousPublicKeys(settings.PreviousPublicKeyPems);
                var jwksJson = BuildJwksJson(currentPublicKey, previousKeys);

                return new MaterializedKeys(true, signingCredentials, currentPublicKey, jwksJson);
            }

            if (string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                throw new InvalidOperationException(
                    "Jwt: configure RsaPrivateKeyPem (recomendado para JWKS) ou SecretKey para HMAC.");
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var hmacCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            return new MaterializedKeys(false, hmacCredentials, symmetricKey, string.Empty);
        }

        private static IReadOnlyList<RsaSecurityKey> LoadPreviousPublicKeys(
            List<JwtSettings.PreviousRsaKey> previousKeyPems)
        {
            var keys = new List<RsaSecurityKey>(previousKeyPems.Count);
            foreach (var entry in previousKeyPems)
            {
                if (string.IsNullOrWhiteSpace(entry.PublicKeyPem) || string.IsNullOrWhiteSpace(entry.KeyId))
                    continue;

                var normalizedPem = entry.PublicKeyPem
                    .Replace("\\n", "\n", StringComparison.Ordinal)
                    .Trim();

                using var rsa = RSA.Create();
                rsa.ImportFromPem(normalizedPem);
                keys.Add(new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = entry.KeyId });
            }
            return keys;
        }

        private static string BuildJwksJson(
            RsaSecurityKey currentPublicKey,
            IReadOnlyList<RsaSecurityKey> previousKeys)
        {
            var keyEntries = new List<string>(previousKeys.Count + 1)
            {
                SerializeSingleRsaKey(currentPublicKey)
            };

            foreach (var key in previousKeys)
                keyEntries.Add(SerializeSingleRsaKey(key));

            return $"{{\"keys\":[{string.Join(",", keyEntries)}]}}";
        }

        private static string SerializeSingleRsaKey(RsaSecurityKey publicKey)
        {
            var p = publicKey.Parameters;
            if (p.Modulus is null || p.Exponent is null)
                throw new InvalidOperationException($"Chave RSA pública inválida para JWKS (kid={publicKey.KeyId}).");

            var kid = publicKey.KeyId ?? "moope-auth-1";
            var n = Base64UrlEncoder.Encode(p.Modulus);
            var e = Base64UrlEncoder.Encode(p.Exponent);
            return $$"""{"kty":"RSA","use":"sig","kid":"{{kid}}","alg":"RS256","n":"{{n}}","e":"{{e}}"}""";
        }

        private sealed record MaterializedKeys(
            bool UsesRsa,
            SigningCredentials SigningCredentials,
            SecurityKey IssuerSigningKey,
            string JwksJson);
    }
}
