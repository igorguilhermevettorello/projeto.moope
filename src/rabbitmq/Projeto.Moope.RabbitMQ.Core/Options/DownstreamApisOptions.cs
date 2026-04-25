namespace Projeto.Moope.RabbitMQ.Core.Options
{
    public sealed class DownstreamApisOptions
    {
        public const string SectionName = "DownstreamApis";

        public string? Auth { get; init; }
        public string? AuthClientId { get; init; }
        public string? AuthClientSecret { get; init; }

        public string? Cliente { get; init; }
        public string? ClienteApiKey { get; init; }

        public string? Pedido { get; init; }
        public string? Pagamento { get; init; }
    }
}

