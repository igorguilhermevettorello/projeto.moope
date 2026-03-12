using FluentValidation;
using Projeto.Moope.Auth.Application.Commands.Usuario.Deletar;

namespace Projeto.Moope.Auth.Application.Validators.Usuario.Deletar
{
    public class DeletarUsuarioCommandValidator : AbstractValidator<DeletarUsuarioCommand>
    {
        public DeletarUsuarioCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage("O Id é obrigatório.")
                .Must(id => id != Guid.Empty)
                .WithMessage("O Id informado é inválido.");
        }
    }
}
