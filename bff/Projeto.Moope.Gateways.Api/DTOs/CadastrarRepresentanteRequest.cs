using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Gateways.Api.DTOs
{
    public class CadastrarRepresentanteRequest
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }

        [Documento("TipoPessoa")]
        public string CpfCnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório")]
        public RepresentanteEnderecoRequest Endereco { get; set; } = null!;

        public string? NomeFantasia { get; set; }

        public string? InscricaoEstadual { get; set; }

        [Required(ErrorMessage = "O percentual de comissão é obrigatório")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal PercentualComissao { get; set; }

        [Required(ErrorMessage = "A chave Pix é obrigatória")]
        [MaxLength(255)]
        public string ChavePix { get; set; } = string.Empty;

        public bool Status { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatório")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Confirmação é obrigatório")]
        [Compare("Senha", ErrorMessage = "A confirmação deve ser igual à senha")]
        public string Confirmacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código do cupom é obrigatório")]
        [MaxLength(100)]
        public string CodigoCupom { get; set; } = string.Empty;
    }
}
