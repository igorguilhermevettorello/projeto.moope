using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Gateways.Api.DTOs
{
    public class CadastrarClienteRequest
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }

        [Documento("TipoPessoa")]
        public string CpfCnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "O endereço é obrigatório")]
        public RepresentanteEnderecoRequest Endereco { get; set; } = null!;

        [Required(ErrorMessage = "O campo Senha é obrigatório")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Confirmação é obrigatório")]
        [Compare("Senha", ErrorMessage = "A confirmação deve ser igual à senha")]
        public string Confirmacao { get; set; } = string.Empty;

        public string NomeFantasia { get; set; } = string.Empty;

        public string InscricaoEstadual { get; set; } = string.Empty;

        public Guid? VendedorId { get; set; }
    }
}
