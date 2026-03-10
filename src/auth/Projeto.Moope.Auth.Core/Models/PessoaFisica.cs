using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Auth.Core.Models
{
    [Table("PessoaFisica")]
    public class PessoaFisica : Entity, IAggregateRoot
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
