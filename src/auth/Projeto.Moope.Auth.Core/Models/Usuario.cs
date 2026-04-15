using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Auth.Core.Models
{
    [Table("Usuario")]
    public class Usuario : Entity, IAggregateRoot
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
