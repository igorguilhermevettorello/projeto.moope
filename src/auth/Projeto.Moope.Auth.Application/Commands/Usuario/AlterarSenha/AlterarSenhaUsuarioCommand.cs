using MediatR;
using Projeto.Moope.Auth.Application.Validators.Usuario.AlterarSenha;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Commands;
using Projeto.Moope.Core.Messages;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.AlterarSenha
{
    public class AlterarSenhaUsuarioCommand : Command<Result>
    {
        public Guid UsuarioId { get; set; }
        public string SenhaAtual { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
        public string ConfirmacaoNovaSenha { get; set; } = string.Empty;

        public override bool IsValid()
        {
            ValidationResult = new AlterarSenhaUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
