using FluentValidation;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Cliente.Core.Validation;
using Projeto.Moope.Core.Enums;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Validators
{
    public class ClienteValidator : AbstractValidator<ClienteModel>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.TipoPessoa)
                .IsInEnum().WithMessage("O campo {PropertyName} deve ser um valor válido")
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .OverridePropertyName("TipoPessoa");

            When(x => x.TipoPessoa == TipoPessoa.JURIDICA, () =>
            {
                RuleFor(x => x.CpfCnpj)
                    .Must(Documentos.IsValidCnpj)
                    .WithMessage("O CNPJ informado não é válido")
                    .OverridePropertyName("CpfCnpj");
            });

            When(x => x.TipoPessoa == TipoPessoa.FISICA, () =>
            {
                RuleFor(x => x.CpfCnpj)
                    .Must(Documentos.IsValidCpf)
                    .WithMessage("O CPF informado não é válido")
                    .OverridePropertyName("CpfCnpj");
            });
        }
    }
}
