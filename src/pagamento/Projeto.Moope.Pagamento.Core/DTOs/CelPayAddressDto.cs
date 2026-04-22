using System.Text.Json.Serialization;

namespace Projeto.Moope.Pagamento.Core.DTOs
{
    public class CelPayAddressDto
    {
        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("complement")]
        public string? Complement { get; set; }

        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
