using Projeto.Moope.Auth.Application.Validators.Usuario.Alterar;
using Projeto.Moope.Auth.Application.Validators.Usuario.Criar;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Alterar
{
    public class AlterarUsuarioCommand : Command<Result<Guid>>
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public TipoPessoa TipoPessoa { get; set; }
        public string NomeFantasia { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public Guid? VendedorId { get; set; }
        public override bool IsValid()
        {
            ValidationResult = new AlterarUsuarioCommandValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
