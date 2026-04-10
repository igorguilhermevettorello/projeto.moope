using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Gateways.Api.DTOs
{
    public sealed class ProcessarVendaRequest
    {
        [Required(ErrorMessage = "Nome do cliente e obrigatorio")]
        public string NomeCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email e obrigatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone e obrigatorio")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo TipoPessoa e obrigatorio")]
        public TipoPessoa TipoPessoa { get; set; }

        [Documento("TipoPessoa")]
        public string? CpfCnpj { get; set; }

        public Guid? VendedorId { get; set; }

        public string? CodigoCupom { get; set; }

        [Required(ErrorMessage = "ID do plano e obrigatorio")]
        public Guid? PlanoId { get; set; }

        [Required(ErrorMessage = "Quantidade e obrigatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "Nome do cartao de credito e obrigatorio")]
        public string NomeCartao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numero do cartao e obrigatorio")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Numero do cartao deve ter entre 13 e 19 digitos")]
        public string NumeroCartao { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV e obrigatorio")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV deve ter 3 ou 4 digitos")]
        public string Cvv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de validade e obrigatoria")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
        public string DataValidade { get; set; } = string.Empty;

        public string? Estado { get; set; }

        public List<string>? Descontos { get; set; }

        public string? ComodatoToken { get; set; }
    }
}
