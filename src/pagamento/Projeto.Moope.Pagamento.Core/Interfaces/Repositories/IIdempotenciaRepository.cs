using Projeto.Moope.Core.Interfaces.Data;
using IdempotenciaModel = Projeto.Moope.Pagamento.Core.Models.IdempotenciaPagamento;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Repositories
{
    public interface IIdempotenciaRepository : IRepository<IdempotenciaModel>
    {
        Task<IdempotenciaModel?> ObterPorChaveEscopoAsync(string idempotencyKey, string scope, CancellationToken cancellationToken);
        Task AdicionarAsync(IdempotenciaModel idempotencia, CancellationToken cancellationToken);
        Task AtualizarAsync(IdempotenciaModel idempotencia, CancellationToken cancellationToken);
    }
}

