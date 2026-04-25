namespace Projeto.Moope.RabbitMQ.Core.DTOs.Pedido
{
    public sealed class PedidoValoresPagamentoDto
    {
        public Guid PedidoId { get; init; }
        public int Quantidade { get; init; }

        public decimal TaxaAdesaoUnitaria { get; init; }
        public decimal ValorTotalTaxaAdesao { get; init; }

        public decimal MensalidadeUnitaria { get; init; }
        public decimal ValorTotalMensalidade { get; init; }
    }
}

