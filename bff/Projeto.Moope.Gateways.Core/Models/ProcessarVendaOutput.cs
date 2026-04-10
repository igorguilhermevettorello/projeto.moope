namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class ProcessarVendaOutput
    {
        public Guid PlanoId { get; init; }

        public int Quantidade { get; init; }

        public decimal ValorTotal { get; init; }

        public Guid VendaId { get; init; }

        public Guid TransacaoId { get; init; }
    }
}
