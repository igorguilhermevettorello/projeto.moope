using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Services
{
    public class ClienteService : BaseService, IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(
            IClienteRepository clienteRepository,
            INotificador notificador) : base(notificador)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ClienteModel?> BuscarPorIdAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsync(id);
        }

        public async Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>()
        {
            return await _clienteRepository.BuscarClientesComDadosAsync<T>();
        }

        public async Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id)
        {
            return await _clienteRepository.BuscarClientePorIdComDadosAsync<T>(id);
        }

        public async Task<ClienteModel?> BuscarPorCodigoCupomAsync(string codigoCupom)
        {
            return await _clienteRepository.BuscarPorCodigoCupomAsync(codigoCupom);
        }

        public async Task<IEnumerable<ClienteModel>> BuscarTodosAsync()
        {
            return await _clienteRepository.BuscarTodosAsync();
        }

        public async Task<Result<ClienteModel>> SalvarAsync(ClienteModel cliente)
        {
            var agora = DateTime.UtcNow;
            cliente.Created = agora;
            cliente.Updated = agora;

            var entity = await _clienteRepository.SalvarAsync(cliente);
            return new Result<ClienteModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<ClienteModel>> AtualizarAsync(ClienteModel cliente)
        {
            cliente.Updated = DateTime.UtcNow;

            var entity = await _clienteRepository.AtualizarAsync(cliente);
            return new Result<ClienteModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _clienteRepository.RemoverAsync(id);
            return true;
        }
    }
}
