using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Auth.Core.DTOs.Usuario
{
    public class DeletarUsuarioDto
    {
        [Required(ErrorMessage = "O campo ID é obrigatório")]
        public Guid Id { get; set; }
    }
}
