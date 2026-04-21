namespace Projeto.Moope.Pagamento.Infrastructure.Configurations
{
    public class CelcoinPaymentsSettings
    {
        public const string SectionName = "CelcoinPayments";
        public string BaseUrl { get; set; } = "https://api.sandbox.cel.cash/v2";
        public string GalaxId { get; set; } = string.Empty;
        public string GalaxHash { get; set; } = string.Empty;
        public string? PartnerGalaxId { get; set; }
        public string? PartnerGalaxHash { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int TokenRefreshSkewSeconds { get; set; } = 30;
    }
}

