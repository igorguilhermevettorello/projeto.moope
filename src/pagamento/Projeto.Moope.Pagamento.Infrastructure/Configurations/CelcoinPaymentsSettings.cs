namespace Projeto.Moope.Pagamento.Infrastructure.Configurations
{
    public class CelcoinPaymentsSettings
    {
        public const string SectionName = "CelcoinPayments";

        public string BaseUrl { get; set; } = "https://api.sandbox.cel.cash/v2";

        /// <summary>Galax Id do webservice.</summary>
        public string GalaxId { get; set; } = string.Empty;

        /// <summary>Galax Hash do webservice.</summary>
        public string GalaxHash { get; set; } = string.Empty;

        /// <summary>Opcional: autenticação como parceiro (AuthorizationPartner).</summary>
        public string? PartnerGalaxId { get; set; }

        /// <summary>Opcional: autenticação como parceiro (AuthorizationPartner).</summary>
        public string? PartnerGalaxHash { get; set; }

        /// <summary>Timeout HTTP em segundos.</summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Antecipação de renovação do token (segundos) para evitar expiração em requisições concorrentes.
        /// Token do gateway tem validade típica de 600s.
        /// </summary>
        public int TokenRefreshSkewSeconds { get; set; } = 30;
    }
}

