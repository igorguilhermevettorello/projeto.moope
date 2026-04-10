using Projeto.Moope.Auth.Application.Validators.Usuario.AtualizarEndereco;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Messages;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.AtualizarEndereco
{
    public class AtualizarEnderecoUsuarioCommand : Command<Result>
    {
        public Guid UsuarioId { get; set; }
        public Guid EnderecoId { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new AtualizarEnderecoUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
