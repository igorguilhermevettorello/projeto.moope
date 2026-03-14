using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Infrastructure.Data;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Infrastructure.Repositories
{
    public class ComodatoRepository : IComodatoRepository
    {
        private readonly AppComodatoContext _context;

        public ComodatoRepository(AppComodatoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<ComodatoModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Comodatos.FindAsync(id))!;
        }

        public async Task<ComodatoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Comodatos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ComodatoModel>> BuscarPorClienteIdAsync(Guid clienteId)
        {
            return await _context.Comodatos.AsNoTracking()
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ComodatoModel>> BuscarTodosAsync()
        {
            return await _context.Comodatos.ToListAsync();
        }

        public async Task<ComodatoModel> SalvarAsync(ComodatoModel entity)
        {
            await _context.Comodatos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ComodatoModel> AtualizarAsync(ComodatoModel entity)
        {
            _context.Comodatos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Comodatos.FindAsync(id);
            if (entity == null)
                return false;

            _context.Comodatos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
