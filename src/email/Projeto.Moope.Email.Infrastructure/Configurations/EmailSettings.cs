namespace Projeto.Moope.Email.Infrastructure.Configurations
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string Usuario { get; set; } = string.Empty;
        public string RemetenteEmail { get; set; } = string.Empty;
        public string RemetenteNome { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// URL base da API de envio de email (ex: https://mailserver.moope.com.br)
        /// </summary>
        public string ApiUrl { get; set; } = "https://mailserver.moope.com.br/";

        /// <summary>
        /// Chave de API para autenticação no endpoint de envio
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Caminho do endpoint de envio (ex: enviar-email). O request será feito para ApiUrl + EndpointEnvio
        /// </summary>
        public string EndpointEnvio { get; set; } = "enviar-email";
    }
}
