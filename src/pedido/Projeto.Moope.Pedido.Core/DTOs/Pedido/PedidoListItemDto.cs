namespace Projeto.Moope.Pedido.Core.DTOs.Pedido
{
    public class PedidoListItemDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public string Plano { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
        public decimal Total { get; set; }
    }
}
