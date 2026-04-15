using MediatR;
using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.Cliente.Core.Commands.Clientes.AlterarSenha
{
    public class AlterarSenhaAdminCommand : IRequest<Result>
    {
        public Guid ClienteId { get; }
        public string NovaSenha { get; }

        public AlterarSenhaAdminCommand(Guid clienteId, string novaSenha)
        {
            ClienteId = clienteId;
            NovaSenha = novaSenha;
        }
    }
}
