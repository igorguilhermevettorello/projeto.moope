using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Comodato.Core.Models
{
    public class ComodatoConvite : Entity
    {
        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = default!;

        public Guid CreatedByAdminId { get; set; }

        public Guid PlanoId { get; set; }
        //public Plano Plano { get; set; } = default!;

        public int Quantidade { get; set; }

        public decimal Valor { get; set; }

        public Guid? VendedorId { get; set; }
        //public Vendedor? Vendedor { get; set; }

        public DateTime CriadoEm { get; set; }

        public DateTime ExpiradoEm { get; set; }

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

        public string Estado { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
