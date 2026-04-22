using System.Text.Json.Serialization;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pagamento.Core.Utils;

namespace Projeto.Moope.Pagamento.Core.DTOs
{
    public class CelPayAssinaturaSemPlanoRequestDto
    {
        [JsonPropertyName("myId")]
        public string? MyId { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public int Value { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 0;
        [JsonPropertyName("periodicity")]
        [JsonConverter(typeof(LowercaseEnumConverter<Periodicidade>))]
        public Periodicidade Periodicity { get; set; }  // monthly, yearly, etc.
        [JsonPropertyName("firstPayDayDate")]
        public string FirstPayDayDate { get; set; } = string.Empty; // YYYY-MM-DD
        [JsonPropertyName("additionalInfo")]
        public string AdditionalInfo { get; set; } = string.Empty;
        [JsonPropertyName("mainPaymentMethodId")]
        [JsonConverter(typeof(LowercaseEnumConverter<MetodoPagamento>))]
        public MetodoPagamento MainPaymentMethodId { get; set; }
        [JsonPropertyName("Customer")]
        public CelPayCustomerDto Customer { get; set; } = new();
        [JsonPropertyName("Transactions")]
        public List<CelPayTransactionDto> Transactions { get; set; } = new();
        [JsonPropertyName("PaymentMethodCreditCard")]
        public CelPayCardDto Card { get; set; }
    }
}
