using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Endereco.Api.DTOs
{
    public class DeletarEnderecoDto
    {
        [Required(ErrorMessage = "O campo ID é obrigatório")]
        public Guid Id { get; set; }
    }
}
