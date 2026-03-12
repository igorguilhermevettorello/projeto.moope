using MediatR;
using Projeto.Moope.Auth.Application.Validators.Usuario.Deletar;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Commands;
using Projeto.Moope.Core.Messages;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Deletar
{
    public class DeletarUsuarioCommand : Command<Result>
    {
        public Guid Id { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new DeletarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
