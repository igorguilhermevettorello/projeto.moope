namespace Projeto.Moope.Gateways.Api.Options
{
    public class DownstreamApisOptions
    {
        public const string SectionName = "DownstreamApis";

        public string Vendedor { get; set; } = string.Empty;

        public string Auth { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string Plano { get; set; } = string.Empty;

        public string Pagamento { get; set; } = string.Empty;

        public string Venda { get; set; } = string.Empty;
    }
}
