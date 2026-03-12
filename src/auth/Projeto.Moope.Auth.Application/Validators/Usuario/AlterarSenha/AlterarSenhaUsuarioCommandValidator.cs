using FluentValidation;
using Projeto.Moope.Auth.Application.Commands.Usuario.AlterarSenha;

namespace Projeto.Moope.Auth.Application.Validators.Usuario.AlterarSenha
{
    public class AlterarSenhaUsuarioCommandValidator : AbstractValidator<AlterarSenhaUsuarioCommand>
    {
        public AlterarSenhaUsuarioCommandValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty()
                .WithMessage("O Id do usuário é obrigatório.")
                .Must(id => id != Guid.Empty)
                .WithMessage("O Id informado é inválido.");

            RuleFor(c => c.SenhaAtual)
                .NotEmpty()
                .WithMessage("O campo Senha Atual é obrigatório.");

            RuleFor(c => c.NovaSenha)
                .NotEmpty()
                .WithMessage("O campo Nova Senha é obrigatório.")
                .MinimumLength(6)
                .WithMessage("A nova senha deve ter no mínimo 6 caracteres.");

            RuleFor(c => c.ConfirmacaoNovaSenha)
                .NotEmpty()
                .WithMessage("O campo Confirmação da Nova Senha é obrigatório.")
                .Equal(c => c.NovaSenha)
                .When(c => !string.IsNullOrEmpty(c.NovaSenha))
                .WithMessage("A confirmação da nova senha não confere.");
        }
    }
}
