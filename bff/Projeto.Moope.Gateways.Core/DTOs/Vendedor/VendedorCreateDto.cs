using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;

namespace Projeto.Moope.Gateways.Core.DTOs.Vendedor
{
    public sealed class VendedorCreateDto
    {
        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public TipoPessoa TipoPessoa { get; set; }

        public string CpfCnpj { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public EnderecoCreateDto Endereco { get; set; } = null!;

        public string? NomeFantasia { get; set; }

        public string? InscricaoEstadual { get; set; }

        public decimal PercentualComissao { get; set; }

        public string ChavePix { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public string Confirmacao { get; set; } = string.Empty;

        public string CodigoCupom { get; set; } = string.Empty;
    }
}
