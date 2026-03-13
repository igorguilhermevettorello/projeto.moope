using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Plano.Core.Interfaces.Repositories;
using Projeto.Moope.Plano.Infrastructure.Data;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Infrastructure.Repositories
{
    public class PlanoRepository : IPlanoRepository
    {
        private readonly AppPlanoContext _context;

        public PlanoRepository(AppPlanoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<PlanoModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Planos.FindAsync(id))!;
        }

        public async Task<PlanoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PlanoModel?> BuscarPorPlanoSelecionadoAsync(string codigo)
        {
            return await _context.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<IEnumerable<PlanoModel>> BuscarTodosAsync()
        {
            return await _context.Planos.ToListAsync();
        }

        public async Task<PlanoModel> SalvarAsync(PlanoModel entity)
        {
            await _context.Planos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PlanoModel> AtualizarAsync(PlanoModel entity)
        {
            _context.Planos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Planos.FindAsync(id);
            if (entity == null)
                return false;

            _context.Planos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
