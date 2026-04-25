using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Pagamento.Core.Enums;

namespace Projeto.Moope.Pagamento.Core.Models
{
    [Table("IntencaoPagamento")]
    public class IntencaoPagamento : Entity, IAggregateRoot
    {
        public decimal Valor { get; set; }

        public string Moeda { get; set; } = string.Empty;

        public StatusIntencaoPagamento Status { get; set; }

        public MetodoPagamento MetodoPagamento { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
