using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Projeto.Moope.Pagamento.Core.Utils;

namespace Projeto.Moope.Pagamento.Core.DTOs
{
    public class CelPayCustomerDto
    {
        [JsonPropertyName("myId")]
        public string? MyId { get; set; }
        [JsonPropertyName("galaxPayId")]
        public int GalaxPayId { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("document")]
        public string? Document { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new List<string>();
        [JsonPropertyName("phones")]
        [JsonConverter(typeof(FlexibleStringListConverter))]
        public List<string> Phones { get; set; } = new List<string>();
        [JsonPropertyName("Address")]
        public CelPayAddressDto Address { get; set; }
    }
}
