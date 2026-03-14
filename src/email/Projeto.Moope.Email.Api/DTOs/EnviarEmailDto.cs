using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Email.Api.DTOs
{
    /// <summary>
    /// DTO para envio de email via endpoint HTTP (usa IEmailGateway).
    /// </summary>
    public class EnviarEmailDto
    {
        [Required(ErrorMessage = "O campo Destinatario é obrigatório")]
        [EmailAddress(ErrorMessage = "Email do destinatário inválido")]
        public string Destinatario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Assunto é obrigatório")]
        public string Assunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Corpo é obrigatório")]
        public string Corpo { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email do remetente inválido")]
        public string? Remetente { get; set; }

        public string? NomeRemetente { get; set; }
        public string? Copia { get; set; }
        public string? CopiaOculta { get; set; }
        public bool EhHtml { get; set; } = true;
    }
}
