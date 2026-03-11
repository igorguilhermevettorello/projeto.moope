using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class PapelRepository : IPapelRepository
    {
        private readonly AppAuthContext _context;

        public PapelRepository(AppAuthContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<Papel> BuscarPorIdAsync(Guid id)
        {
            return await _context.Papeis.FirstOrDefaultAsync(p => p.Id == id) ?? new Papel();
        }

        public async Task<IEnumerable<Papel>> BuscarTodosAsync()
        {
            return await _context.Papeis.ToListAsync();
        }

        public async Task<IEnumerable<Papel>> BuscarPorUsuarioIdAsync(Guid usuarioId)
        {
            return await _context.Papeis
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Papel> SalvarAsync(Papel entity)
        {
            await _context.Papeis.AddAsync(entity);
            return entity;
        }

        public async Task<Papel> AtualizarAsync(Papel entity)
        {
            _context.Papeis.Update(entity);
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pedido = await _context.Papeis.FindAsync(id);
            if (pedido != null)
            {
                _context.Papeis.Remove(pedido);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
