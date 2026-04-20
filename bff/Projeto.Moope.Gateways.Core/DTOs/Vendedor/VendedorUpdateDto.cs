using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;

namespace Projeto.Moope.Gateways.Core.DTOs.Vendedor
{
    public sealed class VendedorUpdateDto
    {
        public Guid Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public TipoPessoa TipoPessoa { get; set; }

        public string CpfCnpj { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string? NomeFantasia { get; set; }

        public string? InscricaoEstadual { get; set; }

        public Guid? VendedorId { get; set; }

        public decimal PercentualComissao { get; set; }

        public string ChavePix { get; set; } = string.Empty;

        public string CodigoCupom { get; set; } = string.Empty;

        public EnderecoUpdateDto? Endereco { get; set; }
    }
}
