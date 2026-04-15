using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Cliente.Core.Models
{
    [Table("Cliente")]
    public class Cliente : Entity
    {
        [NotMapped]
        public TipoPessoa TipoPessoa { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        [NotMapped]
        public string CpfCnpj { get; set; } = string.Empty;

        public string? Telefone { get; set; }
        public string? TelefoneEmergencia { get; set; }
        public Guid? VendedorId { get; set; }
    }
}
