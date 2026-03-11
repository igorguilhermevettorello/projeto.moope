using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class PessoaFisicaRepository : IPessoaFisicaRepository
    {
        private readonly AppAuthContext _context;

        public PessoaFisicaRepository(AppAuthContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork) _context;

        public async Task<PessoaFisica> BuscarPorIdAsync(Guid id)
        {
            return await _context.PessoasFisicas.FirstOrDefaultAsync(pf => pf.Id == id);
        }

        public async Task<IEnumerable<PessoaFisica>> BuscarTodosAsync()
        {
            return await _context.PessoasFisicas.ToListAsync();
        }

        public async Task<PessoaFisica> SalvarAsync(PessoaFisica entity)
        {
            await _context.PessoasFisicas.AddAsync(entity);
            return entity;
        }

        public async Task<PessoaFisica> AtualizarAsync(PessoaFisica entity)
        {
            _context.PessoasFisicas.Update(entity);
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pessoaFisica = await _context.PessoasFisicas.FindAsync(id);
            if (pessoaFisica != null)
            {
                _context.PessoasFisicas.Remove(pessoaFisica);
                return true;
            }
            return false;
        }

        public async Task<PessoaFisica> BuscarPorCpfAsync(string cpf)
        {
            return await _context.PessoasFisicas.FirstOrDefaultAsync(pf => pf.Cpf.Equals(cpf));
        }

        public async Task<PessoaFisica> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.PessoasFisicas.AsNoTracking().FirstOrDefaultAsync(pf => pf.Id == id);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
