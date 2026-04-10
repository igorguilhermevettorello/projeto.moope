using Projeto.Moope.Core.DTOs;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Core.Interfaces.Services
{
    public interface IVendedorService
    {
        Task<VendedorModel?> BuscarPorIdAsync(Guid id);

        Task<VendedorModel?> BuscarPorIdAsNotrackingAsync(Guid id);

        Task<VendedorModel?> BuscarPorCodigoCupomAsync(string codigoCupom);

        Task<IEnumerable<VendedorModel>> BuscarTodosAsync();

        Task<Result<VendedorModel>> SalvarAsync(VendedorModel vendedor);

        Task<Result<VendedorModel>> AtualizarAsync(VendedorModel vendedor);

        Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>();
        Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id);
        Task<bool> RemoverAsync(Guid id);
    }
}
