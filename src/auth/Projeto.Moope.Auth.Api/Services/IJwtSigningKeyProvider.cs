using Microsoft.IdentityModel.Tokens;

namespace Projeto.Moope.Auth.Api.Services
{
    public interface IJwtSigningKeyProvider
    {
        bool UsesRsa { get; }

        SigningCredentials GetSigningCredentials();

        SecurityKey GetIssuerSigningKey();

        string GetJwksJson();
    }
}
