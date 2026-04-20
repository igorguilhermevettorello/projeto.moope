using Projeto.Moope.Core.Models;
using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Pedido.Core.Models
{
    [Table("Desconto")]
    public class Desconto : Entity
    {
        public Guid PedidoId { get; set; }
        public decimal ValorPercentual { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public TipoPessoaDesconto TipoPessoa { get; set; }
        public string CodigoDesconto { get; set; } = string.Empty;
        public decimal? ValorDesconto { get; set; } // Valor calculado do desconto em reais
        public bool Ativo { get; set; } = true;

        // Navegação
        public virtual Pedido? Pedido { get; set; }
    }
}
