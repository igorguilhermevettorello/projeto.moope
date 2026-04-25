using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Pedido.Core.Models
{
    [Table("IdempotenciaPedido")]
    public class IdempotenciaPedido : Entity, IAggregateRoot
    {
        public string IdempotencyKey { get; set; } = string.Empty;

        public string Scope { get; set; } = string.Empty;

        public string RequestHash { get; set; } = string.Empty;

        public StatusIdempotencia Status { get; set; }

        public int? ResponseStatusCode { get; set; }

        public string? ResponseBody { get; set; }

        public string? ResourceId { get; set; }

        public string? ResourceType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

