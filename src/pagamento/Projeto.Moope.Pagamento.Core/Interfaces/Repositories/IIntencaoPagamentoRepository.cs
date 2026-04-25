using Projeto.Moope.Pagamento.Core.Models;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Repositories
{
    public interface IIntencaoPagamentoRepository
    {
        Task<IntencaoPagamento> AdicionarAsync(IntencaoPagamento intencao, CancellationToken cancellationToken = default);

        Task<IntencaoPagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
