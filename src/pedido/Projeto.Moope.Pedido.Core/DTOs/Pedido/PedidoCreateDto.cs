using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Pedido.Core.DTOs.Pedido
{
    public class PedidoCreateDto
    {
        public Guid ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public Guid PlanoId { get; set; }        
        public int Quantidade { get; set; }
        public TipoPessoa TipoPessoa { get; set; }
        public string? Estado { get; set; }
    }
}
