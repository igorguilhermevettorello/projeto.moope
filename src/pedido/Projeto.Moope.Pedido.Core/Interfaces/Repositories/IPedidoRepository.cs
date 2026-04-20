using Projeto.Moope.Core.Interfaces.Data;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Core.Interfaces.Repositories
{
    public interface IPedidoRepository : IRepository<PedidoModel>
    {
        Task<PedidoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<PedidoModel?> BuscarPorIdComTransacoesEDescontoAsync(Guid id);
        Task<IEnumerable<TransacaoModel>> BuscarTransacoesPorPedidoIdAsync(Guid pedidoId);
        Task<bool> RemoverTransacoesPorPedidoIdAsync(Guid pedidoId);
        Task<IEnumerable<TransacaoModel>> SalvarTransacoesAsync(IEnumerable<TransacaoModel> transacoes);
    }
}

