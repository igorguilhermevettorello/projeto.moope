using Projeto.Moope.Core.Interfaces.Data;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Core.Interfaces.Repositories
{
    public interface IVendedorRepository : IRepository<VendedorModel>
    {
        Task<VendedorModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<VendedorModel?> BuscarPorCodigoCupomAsync(string codigoCupom);
        //Task<IEnumerable<VendedorModel>> BuscarTodosComDadosRelacionadosAsync();
        Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>();
        Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id);
    }
}
