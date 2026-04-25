using System.Text.Json.Serialization;

namespace Projeto.Moope.RabbitMQ.Core.DTOs.Common
{
    public sealed class ApiResponseEnvelope<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}

