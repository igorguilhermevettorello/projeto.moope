using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Projeto.Moope.Pagamento.Core.DTOs
{
    public class CelPayTransactionDto
    {
        [JsonPropertyName("myId")]
        public string? MyId { get; set; }

        [JsonPropertyName("installment")]
        public int Installment { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("payday")]
        public string? Payday { get; set; }

        [JsonPropertyName("payedOutsideGalaxPay")]
        public bool PayedOutsideGalaxPay { get; set; }

        [JsonPropertyName("additionalInfo")]
        public string? AdditionalInfo { get; set; }
    }
}
