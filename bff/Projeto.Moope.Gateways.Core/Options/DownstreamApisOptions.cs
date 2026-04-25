namespace Projeto.Moope.Gateways.Core.Options
{
    public class DownstreamApisOptions
    {
        public const string SectionName = "DownstreamApis";

        public string Vendedor { get; set; } = string.Empty;

        public string Auth { get; set; } = string.Empty;

        public string AuthClientId { get; set; } = string.Empty;

        public string AuthClientSecret { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;
        
        public string ClienteApiKey { get; set; } = string.Empty;

        public string Plano { get; set; } = string.Empty;

        public string Pagamento { get; set; } = string.Empty;

        public string PagamentoIdempotencyKeyGeneratorApiKey { get; set; } = string.Empty;

        public string Pedido { get; set; } = string.Empty;
    }
}
