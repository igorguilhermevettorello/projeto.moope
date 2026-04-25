using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Infrastructure.Data;
using IdempotenciaModel = Projeto.Moope.Pagamento.Core.Models.IdempotenciaPagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly AppPagamentoContext _context;

        public IdempotenciaRepository(AppPagamentoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<IdempotenciaModel?> ObterPorChaveEscopoAsync(string idempotencyKey, string scope, CancellationToken cancellationToken)
        {
            return await _context.Idempotencias
                .FirstOrDefaultAsync(i => i.IdempotencyKey == idempotencyKey && i.Scope == scope, cancellationToken);
        }

        public async Task AdicionarAsync(IdempotenciaModel idempotencia, CancellationToken cancellationToken)
        {
            await _context.Idempotencias.AddAsync(idempotencia, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AtualizarAsync(IdempotenciaModel idempotencia, CancellationToken cancellationToken)
        {
            _context.Idempotencias.Update(idempotencia);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IdempotenciaModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Idempotencias.FindAsync(id))!;
        }

        public async Task<IEnumerable<IdempotenciaModel>> BuscarTodosAsync()
        {
            return await _context.Idempotencias.AsNoTracking().ToListAsync();
        }

        public async Task<IdempotenciaModel> SalvarAsync(IdempotenciaModel entity)
        {
            await _context.Idempotencias.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IdempotenciaModel> AtualizarAsync(IdempotenciaModel entity)
        {
            _context.Idempotencias.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Idempotencias.FindAsync(id);
            if (entity == null)
                return false;

            _context.Idempotencias.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

