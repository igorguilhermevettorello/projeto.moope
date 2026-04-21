using System.Text.Json;

namespace Projeto.Moope.Pagamento.Api.DTOs
{
    public class GatewayAuthRequestDto
    {
        public string Scope { get; set; } = string.Empty;
    }

    public class CriarClienteRequestDto
    {
        public Guid? ClienteId { get; set; }
        public JsonElement Payload { get; set; }
    }

    public class CriarPlanoRequestDto
    {
        public JsonElement Payload { get; set; }
    }

    public class CriarCartaoRequestDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string TypeId { get; set; } = "galaxPayId";
        public JsonElement Payload { get; set; }
    }

    public class CriarAssinaturaRequestDto
    {
        public JsonElement Payload { get; set; }
    }

    public class CriarAssinaturaManualRequestDto
    {
        public JsonElement Payload { get; set; }
    }

    public class AdicionarTransacaoEmAssinaturaRequestDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string TypeId { get; set; } = "galaxPayId";
        public JsonElement Payload { get; set; }
    }

    public class CriarCobrancaAvulsaRequestDto
    {
        public JsonElement Payload { get; set; }
    }
}

