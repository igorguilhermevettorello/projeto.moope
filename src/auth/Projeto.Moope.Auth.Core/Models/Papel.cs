using Projeto.Moope.Auth.Core.Enums;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Auth.Core.Models
{
    [Table("Papel")]
    public class Papel : Entity, IAggregateRoot
    {
        public Guid? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        [Required]
        public TipoUsuario TipoUsuario { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
