using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Infrastructure.Data;
using IntencaoModel = Projeto.Moope.Pagamento.Core.Models.IntencaoPagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Repositories
{
    public class IntencaoPagamentoRepository : IIntencaoPagamentoRepository
    {
        private readonly AppPagamentoContext _contexto;

        public IntencaoPagamentoRepository(AppPagamentoContext contexto)
        {
            _contexto = contexto;
        }

        public async Task<IntencaoModel> AdicionarAsync(
            IntencaoModel intencao,
            CancellationToken cancellationToken = default)
        {
            await _contexto.IntencoesPagamento.AddAsync(intencao, cancellationToken);
            await _contexto.SaveChangesAsync(cancellationToken);
            return intencao;
        }

        public async Task<IntencaoModel?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _contexto.IntencoesPagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
