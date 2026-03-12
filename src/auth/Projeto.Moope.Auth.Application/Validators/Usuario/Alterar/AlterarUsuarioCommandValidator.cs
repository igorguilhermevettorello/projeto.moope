using FluentValidation;
using Projeto.Moope.Auth.Application.Commands.Usuario.Alterar;

namespace Projeto.Moope.Auth.Application.Validators.Usuario.Alterar
{
    public class AlterarUsuarioCommandValidator : AbstractValidator<AlterarUsuarioCommand>
    {
        public AlterarUsuarioCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage("O Id é obrigatório.")
                .Must(id => id != Guid.Empty)
                .WithMessage("O Id informado é inválido.");

            RuleFor(c => c.Email)
                .NotEmpty()
                .WithMessage("O campo Email é obrigatório.")
                .EmailAddress()
                .WithMessage("Email deve ter um formato válido.");

            RuleFor(c => c.CpfCnpj)
                .NotEmpty()
                .WithMessage("O campo Documento é obrigatório.");

            RuleFor(c => c.Telefone)
                .NotEmpty()
                .WithMessage("O campo Telefone é obrigatório.");

            RuleFor(c => c.TipoPessoa)
                .IsInEnum()
                .WithMessage("O tipo de pessoa informado é inválido.");
        }
    }
}
