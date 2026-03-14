using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Comodato.Core.Models
{
    [Table("Comodato")]
    public class Comodato : Entity, IAggregateRoot
    {
        [Required]
        public Guid ClienteId { get; set; }

        [Required]
        [MaxLength(120)]
        public string ProdutoNome { get; set; } = default!;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Valor { get; set; }

        [Required]
        public DateTime CriadoEm { get; set; }

        [Required]
        public ComodatoStatus Status { get; set; }
    }
}
