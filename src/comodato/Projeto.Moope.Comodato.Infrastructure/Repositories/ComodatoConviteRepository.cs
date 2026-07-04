using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Core.Models;
using Projeto.Moope.Comodato.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Comodato.Infrastructure.Repositories
{
    public class ComodatoConviteRepository : IComodatoConviteRepository
    {
        private readonly AppComodatoContext _context;

        public ComodatoConviteRepository(AppComodatoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<ComodatoConvite> BuscarPorIdAsync(Guid id)
        {
            return (await _context.ComodatoConvites.FindAsync(id))!;
        }

        public async Task<ComodatoConvite?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.ComodatoConvites.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ComodatoConvite>> BuscarTodosAsync()
        {
            return await _context.ComodatoConvites.ToListAsync();
        }

        public async Task<ComodatoConvite> SalvarAsync(ComodatoConvite entity)
        {
            await _context.ComodatoConvites.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ComodatoConvite> AtualizarAsync(ComodatoConvite entity)
        {
            _context.ComodatoConvites.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.ComodatoConvites.FindAsync(id);
            if (entity == null)
                return false;

            _context.ComodatoConvites.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
