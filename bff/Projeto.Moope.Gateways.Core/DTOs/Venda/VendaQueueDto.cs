using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Gateways.Core.DTOs.Venda
{
    public class VendaQueueDto
    {
        public string Name { get; init; }
        public string Email { get; init; }
        public Guid PedidoId { get; init; }
        public decimal Valor { get; init; }
        public decimal TaxaAdesao { get; init; }
        public Periodicidade Periodicidade { get; init; }
        public MetodoPagamento MetodoPagamento { get; init; }
        public int GalaxPayCustomerId { get; init; }
        public int GalaxPayCardId { get; init; }
        public string Observacao { get; init; } = string.Empty;
    }
}
