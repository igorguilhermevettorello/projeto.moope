using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Email.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Email.Core.Models
{
    [Table("Email")]
    public class Email : Entity, IAggregateRoot
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Remetente { get; set; } = string.Empty;
        [StringLength(255)]
        public string? NomeRemetente { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Destinatario { get; set; } = string.Empty;
        [StringLength(255)]
        public string? NomeDestinatario { get; set; }
        [StringLength(1000)]
        public string? Copia { get; set; }
        [StringLength(1000)]
        public string? CopiaOculta { get; set; }
        [Required]
        [StringLength(500)]
        public string Assunto { get; set; } = string.Empty;
        [Required]
        public string Corpo { get; set; } = string.Empty;
        public bool EhHtml { get; set; } = true;
        public Prioridade Prioridade { get; set; } = Prioridade.Normal;
        public StatusEmail Status { get; set; } = StatusEmail.Pendente;
        public int TentativasEnvio { get; set; } = 0;
        public DateTime? UltimaTentativa { get; set; }
        public DateTime? DataEnvio { get; set; }
        public string? MensagemErro { get; set; }
        public string? MensagemSucesso { get; set; }
        public Guid? UsuarioId { get; set; }
        public Guid? ClienteId { get; set; }
        [StringLength(100)]
        public string? Tipo { get; set; }
        public string? DadosAdicionais { get; set; }
        public DateTime? DataProgramada { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
