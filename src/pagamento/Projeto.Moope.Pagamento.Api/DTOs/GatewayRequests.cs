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
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
    }

    public class CriarPlanoRequestDto
    {
        public JsonElement Payload { get; set; }
    }

    public class CriarCartaoRequestDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string TypeId { get; set; } = "galaxPayId";
        //public JsonElement Payload { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Holder { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
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

