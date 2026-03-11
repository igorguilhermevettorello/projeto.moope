using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppAuthContext _context;

        public UsuarioRepository(AppAuthContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<Usuario> AtualizarAsync(Usuario entity)
        {
            _context.Usuarios.Update(entity);
            return entity;
        }

        public Task<Dictionary<Guid, string>> BuscarNomesPorIdsAsync(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id) ?? new Usuario();
        }

        public async Task<Usuario> BuscarPorIdAsync(Guid id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id) ?? new Usuario();
        }

        public async Task<IEnumerable<Usuario>> BuscarTodosAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Usuarios.FindAsync(id);
            if (entity != null)
            {
                _context.Usuarios.Remove(entity);
                return true;
            }
            return false;
        }

        public async Task<Usuario> SalvarAsync(Usuario entity)
        {
            await _context.Usuarios.AddAsync(entity);
            return entity;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
