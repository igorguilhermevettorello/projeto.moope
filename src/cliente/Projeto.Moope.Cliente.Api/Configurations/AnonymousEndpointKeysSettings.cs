namespace Projeto.Moope.Cliente.Api.Configurations
{
    public sealed class AnonymousEndpointKeysSettings
    {
        public const string SectionPath = "Security:AnonymousEndpointKeys";

        public string BuscarClientePorEmail { get; set; } = string.Empty;
    }
}
