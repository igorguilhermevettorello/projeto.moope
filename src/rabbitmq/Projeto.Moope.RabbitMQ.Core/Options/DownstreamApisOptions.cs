namespace Projeto.Moope.RabbitMQ.Core.Options
{
    public sealed class DownstreamApisOptions
    {
        public const string SectionName = "DownstreamApis";

        public string? Pagamento { get; init; }
    }
}

