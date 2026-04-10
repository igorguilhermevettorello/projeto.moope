using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class ProcessarVendaInput
    {
        public string NomeCliente { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string Telefone { get; init; } = string.Empty;

        public TipoPessoa TipoPessoa { get; init; }

        public string? CpfCnpj { get; init; }

        public Guid? VendedorId { get; init; }

        public string? CodigoCupom { get; init; }

        public Guid PlanoId { get; init; }

        public int Quantidade { get; init; }

        public string NomeCartao { get; init; } = string.Empty;

        public string NumeroCartao { get; init; } = string.Empty;

        public string Cvv { get; init; } = string.Empty;

        public string DataValidade { get; init; } = string.Empty;

        public string? Estado { get; init; }

        public IReadOnlyList<string>? Descontos { get; init; }

        public string? ComodatoToken { get; init; }
    }
}
