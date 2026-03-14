namespace Projeto.Moope.Email.Core.Interfaces.Services
{
    /// <summary>
    /// Interface para envio de email via requisição HTTP a endpoint externo.
    /// A implementação fica na camada Infrastructure para respeitar SOLID (DIP).
    /// </summary>
    public interface IEmailGateway
    {
        /// <summary>
        /// Envia email através de requisição HTTP ao endpoint configurado.
        /// </summary>
        /// <param name="request">Dados do email a ser enviado</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado do envio (sucesso/falha e mensagens)</returns>
        Task<ResultadoEnvioEmail> EnviarAsync(EnvioEmailRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO de entrada para envio de email via gateway HTTP.
    /// </summary>
    public record EnvioEmailRequest(
        string Destinatario,
        string Assunto,
        string Corpo,
        string? Remetente = null,
        string? NomeRemetente = null,
        string? Copia = null,
        string? CopiaOculta = null,
        bool EhHtml = true
    );

    /// <summary>
    /// Resultado do envio de email via gateway.
    /// </summary>
    public record ResultadoEnvioEmail(
        bool Sucesso,
        string? MensagemErro = null,
        string? MensagemSucesso = null
    );
}
