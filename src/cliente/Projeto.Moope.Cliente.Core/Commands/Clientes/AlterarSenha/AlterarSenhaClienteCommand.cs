using MediatR;
using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.Cliente.Core.Commands.Clientes.AlterarSenha
{
    public class AlterarSenhaClienteCommand : IRequest<Result>
    {
        public Guid ClienteId { get; }
        public string SenhaAtual { get; }
        public string NovaSenha { get; }

        public AlterarSenhaClienteCommand(Guid clienteId, string senhaAtual, string novaSenha)
        {
            ClienteId = clienteId;
            SenhaAtual = senhaAtual;
            NovaSenha = novaSenha;
        }
    }
}
