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
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public TipoPessoa TipoPessoa { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
        public string Senha { get; set; } = string.Empty;
        public string Confirmacao { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public override bool IsValid()
        {
            ValidationResult = new CriarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
