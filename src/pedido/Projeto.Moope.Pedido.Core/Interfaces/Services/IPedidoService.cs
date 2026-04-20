using Projeto.Moope.Core.DTOs;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Core.Interfaces.Services
{
    public interface IPedidoService
    {
        Task<PedidoModel?> BuscarPorIdAsync(Guid id);
        Task<PedidoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<PedidoModel?> BuscarPorIdComDadosAsync(Guid id);
        Task<Result<PedidoModel>> SalvarAsync(PedidoModel pedido);
        Task<Result> AtualizarTransacoesAsync(Guid pedidoId, IEnumerable<TransacaoModel> transacoes);
    }
}

