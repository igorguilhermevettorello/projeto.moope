using Projeto.Moope.Core.Models;
using Projeto.Moope.Pedido.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Pedido.Core.Models
{
    [Table("Pedido")]
    public class Pedido : Entity
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
        public decimal PlanoValorComDesconto { get; set; }
        public decimal Total { get; set; }
        public StatusAssinatura StatusAssinatura { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
