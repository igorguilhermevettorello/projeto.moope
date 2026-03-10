using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Infrastructure.Messages;
using System.Threading.Tasks;

namespace Projeto.Moope.Cliente.Core.Handlers
{
    public class UserCreatedMessageHandler
    {
        private readonly IClienteService _clienteService;

        public UserCreatedMessageHandler(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public async Task Handle(UserCreatedMessage message)
        {
            await _clienteService.CreateCliente(message.UserId, message.Nome, message.Email);
        }
    }
}