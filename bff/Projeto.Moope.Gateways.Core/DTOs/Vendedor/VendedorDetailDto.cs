using Projeto.Moope.Gateways.Core.DTOs.Endereco;

namespace Projeto.Moope.Gateways.Core.DTOs.Vendedor
{
    public sealed class VendedorDetailDto
    {
        public Guid Id { get; set; }

        public string? Nome { get; set; }

        public string? Email { get; set; }

        public string? TipoPessoa { get; set; }

        public string? CpfCnpj { get; set; }

        public string? Telefone { get; set; }

        public bool Ativo { get; set; }

        public string? ChavePix { get; set; }

        public decimal? PercentualComissao { get; set; }

        public string? NomeFantasia { get; set; }

        public string? InscricaoEstadual { get; set; }

        public string? CodigoCupom { get; set; }

        public Guid? EnderecoId { get; set; }

        public EnderecoDetailDto? Endereco { get; set; }
    }
}
