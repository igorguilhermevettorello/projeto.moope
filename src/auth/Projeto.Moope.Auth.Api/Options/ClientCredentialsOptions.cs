namespace Projeto.Moope.Auth.Api.Options
{
    public sealed class ClientCredentialsOptions
    {
        public const string SectionName = "ClientCredentials";

        public List<ClientCredentialsClientOptions> Clients { get; set; } = [];

        public sealed class ClientCredentialsClientOptions
        {
            public string ClienteId { get; set; } = string.Empty;
            public string SecretKey { get; set; } = string.Empty;
        }
    }
}

