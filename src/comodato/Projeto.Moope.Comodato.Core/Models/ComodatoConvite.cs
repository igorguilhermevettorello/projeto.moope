using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Comodato.Core.Models
{
    [Table("ComodatoConvite")]
    public class ComodatoConvite : Entity, IAggregateRoot
    {
        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = default!;

        [Required]
        public Guid CreatedByAdminId { get; set; }

        [Required]
        public Guid PlanoId { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Valor { get; set; }

        public Guid? VendedorId { get; set; }

        [Required]
        public DateTime CriadoEm { get; set; }

        [Required]
        public DateTime ExpiradoEm { get; set; }

        [Required]
        public ComodatoConviteStatus Status { get; set; }

        public DateTime? AbertoEm { get; set; }

        public DateTime? ConsumidoEm { get; set; }

        public Guid? ConsumidoPorClienteId { get; set; }

        [MaxLength(255)]
        public string? ClienteEmail { get; set; }

        [MaxLength(20)]
        public string? ClienteDocumento { get; set; }

        public Guid? ComodatoId { get; set; }

        public Comodato? Comodato { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        public DateTime? DataPagamento { get; set; }
    }
}
