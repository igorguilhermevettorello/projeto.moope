using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Pedido.Core.Models
{
    [Table("Pedido")]
    public class Pedido : Entity, IAggregateRoot
    {
        public Guid ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public Guid PlanoId { get; set; }
        public int Quantidade { get; set; }

        // Snapshot do plano no momento da venda
        public decimal PlanoValor { get; set; }
        public string PlanoDescricao { get; set; }
        public string PlanoCodigo { get; set; }
        public decimal PlanoTaxaAdesao { get; set; }
        public decimal PlanoPercentualDesconto { get; set; }
        //public decimal PlanoValorComDesconto { get; set; }
        public decimal PlanoValorTotal { get; set; }
        public decimal PlanoTaxaAdesaoTotal { get; set; }
        public StatusAssinatura StatusAssinatura { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public TipoPessoa TipoPessoa { get; set; }
        public string? Estado { get; set; }
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public Desconto? Desconto { get; set; }
    }
}
