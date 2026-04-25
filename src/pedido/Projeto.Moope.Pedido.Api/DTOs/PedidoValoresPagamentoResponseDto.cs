namespace Projeto.Moope.Pedido.Api.DTOs
{
    public sealed class PedidoValoresPagamentoResponseDto
    {
        public Guid PedidoId { get; init; }
        public int Quantidade { get; init; }

        public decimal TaxaAdesaoUnitaria { get; init; }
        public decimal ValorTotalTaxaAdesao { get; init; }

        public decimal MensalidadeUnitaria { get; init; }
        public decimal ValorTotalMensalidade { get; init; }
    }
}

