using Projeto.Moope.Core.Interfaces.Data;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<ClienteModel>
    {
        Task<ClienteModel?> BuscarPorIdAsync(Guid id);
        Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<ClienteModel?> BuscarPorCodigoCupomAsync(string codigoCupom);
        Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>();
        Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id);
    }
}
