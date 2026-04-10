using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Vendedor.Core.Models
{
    [Table("Vendedor")]
    public class Vendedor : Entity, IAggregateRoot
    {
        [NotMapped]
        public TipoPessoa TipoPessoa { get; set; }

        [NotMapped]
        public string CpfCnpj { get; set; } = string.Empty;

        public decimal PercentualComissao { get; set; }

        public string ChavePix { get; set; } = string.Empty;

        public string CodigoCupom { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public Guid? VendedorId { get; set; }

        public Vendedor? VendedorPai { get; set; }

        public ICollection<Vendedor> VendedoresFilhos { get; set; } = new List<Vendedor>();

        /// <summary>Enriquecimento em memória; não mapeado para o banco deste BC.</summary>
        [NotMapped]
        public object? Usuario { get; set; }
    }
}
