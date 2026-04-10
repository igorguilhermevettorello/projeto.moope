using FluentValidation;
using Projeto.Moope.Auth.Application.Commands.Usuario.AtualizarEndereco;

namespace Projeto.Moope.Auth.Application.Validators.Usuario.AtualizarEndereco
{
    public class AtualizarEnderecoUsuarioCommandValidator : AbstractValidator<AtualizarEnderecoUsuarioCommand>
    {
        public AtualizarEnderecoUsuarioCommandValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty()
                .WithMessage("O UsuarioId é obrigatório.")
                .Must(id => id != Guid.Empty)
                .WithMessage("O UsuarioId informado é inválido.");

            RuleFor(c => c.EnderecoId)
                .NotEmpty()
                .WithMessage("O EnderecoId é obrigatório.")
                .Must(id => id != Guid.Empty)
                .WithMessage("O EnderecoId informado é inválido.");
        }
    }
}
