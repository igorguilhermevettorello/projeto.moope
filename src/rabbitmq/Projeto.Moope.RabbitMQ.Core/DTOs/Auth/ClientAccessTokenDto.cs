using System.Text.Json.Serialization;

namespace Projeto.Moope.RabbitMQ.Core.DTOs.Auth
{
    public sealed class ClientAccessTokenDto
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")]
        public double ExpiresIn { get; set; }
    }
}

