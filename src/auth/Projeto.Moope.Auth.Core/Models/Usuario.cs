using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Auth.Core.Models
{
    [Table("Usuario")]
    public class Usuario : Entity
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; }
        [Required]
        [StringLength(255)]
        public string Email { get; set; }
        [NotMapped]
        [Required]
        public TipoUsuario TipoUsuario { get; set; }
        public Guid? EnderecoId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

     
        //public Endereco Endereco { get; set; }
        [NotMapped]
        public PessoaFisica PessoaFisica { get; set; }
        [NotMapped]
        public PessoaJuridica PessoaJuridica { get; set; }
        [NotMapped]
        public object IdentityUser { get; set; }
    }
}
