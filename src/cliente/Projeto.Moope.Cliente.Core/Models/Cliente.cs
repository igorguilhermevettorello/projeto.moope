using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Cliente.Core.Models
{
    [Table("Cliente")]
    public class Cliente : Entity, IAggregateRoot
    {
        [NotMapped]
        public TipoPessoa TipoPessoa { get; set; }
        [NotMapped]
        public string CpfCnpj { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? TelefoneEmergencia { get; set; }
        public Guid? EnderecoId { get; set; }
        public Guid? VendedorId { get; set; }
        public int? GalaxPayId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }        
        [NotMapped]
        public object? Usuario { get; set; }
    }
}
