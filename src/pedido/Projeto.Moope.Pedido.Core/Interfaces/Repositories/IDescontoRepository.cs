using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pedido.Core.Models;

namespace Projeto.Moope.Pedido.Core.Interfaces.Repositories
{
    public interface IDescontoRepository : IRepository<Desconto>
    {
        Task<IEnumerable<Desconto>> BuscarPorPedidoIdAsync(Guid pedidoId);
        Task<IEnumerable<Desconto>> BuscarPorCodigoDescontoAsync(string codigoDesconto);
        Task<Desconto?> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}
