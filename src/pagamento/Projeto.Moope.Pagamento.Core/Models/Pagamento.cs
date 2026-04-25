using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Pagamento.Core.Models
{
    [Table("Pagamento")]
    public class Pagamento : Entity, IAggregateRoot
    {
        public Guid ClienteId { get; set; }

        public string GatewayCustomerId { get; set; } = string.Empty;
        public string? GatewayPlanId { get; set; }
        public string? GatewaySubscriptionId { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

