namespace Projeto.Moope.Gateways.Core.DTOs.Pedido
{
    public class PedidoListItemDto
    {
        public Guid Id { get; set; }
        public string Plano { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
        public decimal Total { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string? ClienteCidade { get; set; }
        public string? ClienteEstado { get; set; }
        public string? VendedorNome { get; set; }
    }
}
