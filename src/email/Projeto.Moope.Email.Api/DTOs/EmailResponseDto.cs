using Projeto.Moope.Email.Core.Enums;

namespace Projeto.Moope.Email.Api.DTOs
{
    public class EmailResponseDto
    {
        public Guid Id { get; set; }
        public string Remetente { get; set; } = string.Empty;
        public string? NomeRemetente { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string? NomeDestinatario { get; set; }
        public string? Copia { get; set; }
        public string? CopiaOculta { get; set; }
        public string Assunto { get; set; } = string.Empty;
        public string Corpo { get; set; } = string.Empty;
        public bool EhHtml { get; set; }
        public Prioridade Prioridade { get; set; }
        public StatusEmail Status { get; set; }
        public int TentativasEnvio { get; set; }
        public DateTime? UltimaTentativa { get; set; }
        public DateTime? DataEnvio { get; set; }
        public string? MensagemErro { get; set; }
        public string? MensagemSucesso { get; set; }
        public DateTime? DataProgramada { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
