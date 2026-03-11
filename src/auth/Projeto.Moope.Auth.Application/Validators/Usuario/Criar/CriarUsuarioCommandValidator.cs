using FluentValidation;
using Projeto.Moope.Auth.Application.Commands.Usuario.Criar;

namespace Projeto.Moope.Auth.Application.Validators.Usuario.Criar
{
    public class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
    {
        public CriarUsuarioCommandValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty()
                .WithMessage("O ID do aluno é inválido.");
                

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
                .NotEmpty()
                .WithMessage("O campo Telefone é obrigatório.");

            RuleFor(c => c.Senha)
                .NotEmpty()
                .WithMessage("O campo Senha é obrigatório.");

            RuleFor(c => c.Confirmacao)
                .NotEmpty()
                .WithMessage("O campo Confirmacao é obrigatório.");
        }
    }
}
