using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Plano.Core.Models
{
    [Table("Plano")]
    public class Plano : Entity, IAggregateRoot
    {
        [Required]
        public string Codigo { get; set; }
        [Required]
        public string Descricao { get; set; }
        [Required]
        [Column(TypeName = "numeric(15,2)")]
        public decimal Valor { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal? TaxaAdesao { get; set; }
        public bool Status { get; set; }
        public bool Plataforma { get; set; }
    }
}
