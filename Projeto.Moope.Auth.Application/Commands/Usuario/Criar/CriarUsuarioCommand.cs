using MediatR;
using Projeto.Moope.Auth.Application.Validators.Usuario.Criar;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Commands;
using Projeto.Moope.Core.Messages;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Criar
{
    public class CriarUsuarioCommand : Command<Result<Guid>>
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo CpfCnpj é obrigatório")]
        public string CpfCnpj { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }
        [Required(ErrorMessage = "O campo Senha é obrigatório")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo Confirmação é obrigatório")]
        [Compare("Senha", ErrorMessage = "A confirmação deve ser igual à senha")]
        public string Confirmacao { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public Guid? VendedorId { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new CriarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
