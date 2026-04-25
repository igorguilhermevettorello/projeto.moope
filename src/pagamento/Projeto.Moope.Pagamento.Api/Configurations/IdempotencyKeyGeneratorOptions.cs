namespace Projeto.Moope.Pagamento.Api.Configurations
{
    public class IdempotencyKeyGeneratorOptions
    {
        public const string SectionName = "IdempotencyKeyGenerator";

        public string ApiKey { get; set; } = string.Empty;
    }
}

