using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Api.DTOs.Endereco;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Gateways.Api.DTOs.Vendedor
{
    public class VendedorUpdateRequestDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }

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

        public string? NomeFantasia { get; set; }

        public string? InscricaoEstadual { get; set; }

        public Guid? VendedorId { get; set; }

        [Required(ErrorMessage = "O percentual de comissão é obrigatório")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal PercentualComissao { get; set; }

        [Required(ErrorMessage = "A chave Pix é obrigatória")]
        [MaxLength(255)]
        public string ChavePix { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código do cupom é obrigatório")]
        [MaxLength(100)]
        public string CodigoCupom { get; set; } = string.Empty;

        public EnderecoUpdateRequestDto? Endereco { get; set; }
    }
}
