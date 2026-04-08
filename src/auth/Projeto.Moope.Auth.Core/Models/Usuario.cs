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
        //[NotMapped]
        //[Required]
        //[StringLength(255)]
        //public string Email { get; set; }
        //[NotMapped]
        [Required]
        public TipoUsuario TipoUsuario { get; set; }
        public Guid? EnderecoId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        //[NotMapped]
        //public Guid? PessoaFisicaId { get; set; }
        //[NotMapped]
        //public Guid? PessoaJuridicaId { get; set; }
        //[NotMapped]
        //[ForeignKey("PessoaFisicaId")]
        //public PessoaFisica PessoaFisica { get; set; }
        //[NotMapped]
        //[ForeignKey("PessoaJuridicaId")]
        //public PessoaJuridica PessoaJuridica { get; set; }
        //[NotMapped]
        //public object IdentityUser { get; set; }
    }
}
