using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Models;
using System.Threading.Tasks;

namespace Projeto.Moope.Cliente.Infrastructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task CreateCliente(Guid usuarioId, string nome, string email)
        {
            var cliente = new Cliente(usuarioId, nome, email);
            await _clienteRepository.Add(cliente);
        }
    }
}