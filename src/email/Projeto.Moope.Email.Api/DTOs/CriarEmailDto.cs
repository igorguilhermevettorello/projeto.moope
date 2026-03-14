using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Email.Core.Enums;

namespace Projeto.Moope.Email.Api.DTOs
{
    public class CriarEmailDto
    {
        [Required(ErrorMessage = "O campo Remetente é obrigatório")]
        [EmailAddress(ErrorMessage = "Email do remetente inválido")]
        [StringLength(255)]
        public string Remetente { get; set; } = string.Empty;

        [StringLength(255)]
        public string? NomeRemetente { get; set; }

        [Required(ErrorMessage = "O campo Destinatario é obrigatório")]
        [EmailAddress(ErrorMessage = "Email do destinatário inválido")]
        [StringLength(255)]
        public string Destinatario { get; set; } = string.Empty;

        [StringLength(255)]
        public string? NomeDestinatario { get; set; }

        [StringLength(1000)]
        public string? Copia { get; set; }

        [StringLength(1000)]
        public string? CopiaOculta { get; set; }

        [Required(ErrorMessage = "O campo Assunto é obrigatório")]
        [StringLength(500)]
        public string Assunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Corpo é obrigatório")]
        public string Corpo { get; set; } = string.Empty;

        public bool EhHtml { get; set; } = true;

        public Prioridade Prioridade { get; set; } = Prioridade.Normal;

        public DateTime? DataProgramada { get; set; }
    }
}
