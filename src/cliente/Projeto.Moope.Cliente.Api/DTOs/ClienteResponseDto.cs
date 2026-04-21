using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Cliente.Api.DTOs
{
    public class ClienteResponseDto
    {
        [Required(ErrorMessage = "O campo usuário é obrigatório")]
        public Guid? UsuarioId { get; set; }

        public Guid Id { get; set; }

        public TipoPessoa TipoPessoa { get; set; }

        public string CpfCnpj { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public Guid? VendedorId { get; set; }
    }
}
