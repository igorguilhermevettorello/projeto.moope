using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Cliente.Api.DTOs
{
    public class ClienteUpdateDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }
        public string? Telefone { get; set; }
        public string? TelefoneEmergencia { get; set; }
        public Guid? VendedorId { get; set; }
    }
}
