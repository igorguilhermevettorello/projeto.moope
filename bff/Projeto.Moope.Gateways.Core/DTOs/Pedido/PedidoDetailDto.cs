namespace Projeto.Moope.Gateways.Core.DTOs.Pedido
{
    public class PedidoDetailDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal PlanoTaxaAdesao { get; set; }
    }
}
