using Projeto.Moope.Core.DTOs;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Interfaces.Services
{
    public interface IClienteService
    {
        Task<ClienteModel?> BuscarPorIdAsync(Guid id);
        Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<ClienteModel?> BuscarPorCodigoCupomAsync(string codigoCupom);
        Task<IEnumerable<ClienteModel>> BuscarTodosAsync();
        Task<ResultDto<ClienteModel>> SalvarAsync(ClienteModel cliente);
        Task<ResultDto<ClienteModel>> AtualizarAsync(ClienteModel cliente);
        Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>();
        Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id);
        Task<T?> BuscarClientePorEmailComDadosAsync<T>(string email);
        Task<bool> RemoverAsync(Guid id);
        Task<ResultDto> AtualizarEndereco(Guid clienteId, Guid enderecoId);
        Task<ResultDto> AtualizarGalaxPayId(Guid clienteId, int galaxPayId);
    }
}
