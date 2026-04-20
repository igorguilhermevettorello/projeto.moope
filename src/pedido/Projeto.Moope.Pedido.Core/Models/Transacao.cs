using Projeto.Moope.Core.Models;
using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Pedido.Core.Models
{
    [Table("Transacao")]
    public class Transacao : Entity
    {
        public Guid PedidoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public StatusPagamento StatusPagamento { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public string MetodoPagamento { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public Pedido Pedido { get; set; } = null!;
    }
}
