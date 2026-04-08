using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Projeto.Moope.Auth.Api.Services;
using Projeto.Moope.Auth.Api.Utils;
using System.Text.Json.Serialization;

namespace Projeto.Moope.Auth.Api.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class JwtWellKnownController : ControllerBase
    {
        [HttpGet("/.well-known/jwks.json")]
        [Produces("application/json")]
        public IActionResult Jwks([FromServices] IJwtSigningKeyProvider signingKeys)
        {
            if (!signingKeys.UsesRsa)
            {
                return NotFound();
            }

            return Content(signingKeys.GetJwksJson(), "application/json");
        }

        [HttpGet("/.well-known/openid-configuration")]
        [Produces("application/json")]
        public IActionResult OpenIdConfiguration(
            [FromServices] IJwtSigningKeyProvider signingKeys,
            [FromServices] IOptions<JwtSettings> jwtOptions)
        {
            if (!signingKeys.UsesRsa)
            {
                return NotFound();
            }

            var issuer = jwtOptions.Value.Issuer.TrimEnd('/');
            var document = new OpenIdDiscoveryDocument
            {
                Issuer = issuer,
                JwksUri = $"{issuer}/.well-known/jwks.json",
                TokenEndpoint = $"{issuer}/api/auth/login"
            };

            return Ok(document);
        }
    }

    public sealed class OpenIdDiscoveryDocument
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("response_types_supported")]
        public IReadOnlyList<string> ResponseTypesSupported { get; set; } = ["token"];

        [JsonPropertyName("subject_types_supported")]
        public IReadOnlyList<string> SubjectTypesSupported { get; set; } = ["public"];

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public IReadOnlyList<string> IdTokenSigningAlgValuesSupported { get; set; } = ["RS256"];

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public IReadOnlyList<string> TokenEndpointAuthMethodsSupported { get; set; } = ["client_secret_post"];

        [JsonPropertyName("scopes_supported")]
        public IReadOnlyList<string> ScopesSupported { get; set; } = ["openid"];

        [JsonPropertyName("claims_supported")]
        public IReadOnlyList<string> ClaimsSupported { get; set; } = ["sub", "email", "jti", "iat", "nbf", "role", "perfil"];
    }
}
