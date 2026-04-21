using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Cliente.Api.DTOs
{
    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "O campo usuário é obrigatório")]
        public Guid? UsuarioId { get; set; }
        public string? Telefone { get; set; }
        public string? TelefoneEmergencia { get; set; }
        public Guid? VendedorId { get; set; }
    }
}
