using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Core.DTOs;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Interfaces.Services
{
    public interface IClienteService
    {
        Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<ClienteModel?> BuscarPorIdAsync(Guid id);
        Task<ClienteModel?> BuscarPorEmailAsync(string email);
        Task<IEnumerable<ClienteModel   >> BuscarTodosAsync();
        Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>();
        Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id);
        Task<Result<ClienteModel>> SalvarAsync(ClienteModel cliente);
        Task<Result<ClienteModel>> AtualizarAsync(ClienteModel cliente);
        Task<bool> RemoverAsync(Guid id);
        Task<Result> AlterarSenhaClienteAsync(Guid clienteId, string senhaAtual, string novaSenha);
        Task<Result> AlterarSenhaAdminAsync(Guid clienteId, string novaSenha);
        Task<Result<bool>> AlterarTelefoneEmergencia(ClienteModel cliente);
    }
}
