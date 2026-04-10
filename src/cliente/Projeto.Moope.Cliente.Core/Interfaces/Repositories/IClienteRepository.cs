using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;
namespace Projeto.Moope.Cliente.Core.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<ClienteModel>
    {
        Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>();
        Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id);
    }
}
