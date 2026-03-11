using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class PessoaJuridicaRepository : IPessoaJuridicaRepository
    {
        private readonly AppAuthContext _context;

        public PessoaJuridicaRepository(AppAuthContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork) _context;

        public async Task<PessoaJuridica> BuscarPorIdAsync(Guid id)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Id == id) ?? new PessoaJuridica();
        }

        public async Task<IEnumerable<PessoaJuridica>> BuscarTodosAsync()
        {
            return await _context.PessoasJuridicas.ToListAsync();
        }

        public async Task<PessoaJuridica> SalvarAsync(PessoaJuridica entity)
        {
            await _context.PessoasJuridicas.AddAsync(entity);
            return entity;
        }

        public async Task<PessoaJuridica> AtualizarAsync(PessoaJuridica entity)
        {
            _context.PessoasJuridicas.Update(entity);
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pessoaJuridica = await _context.PessoasJuridicas.FindAsync(id);
            if (pessoaJuridica != null)
            {
                _context.PessoasJuridicas.Remove(pessoaJuridica);
                return true;
            }
            return false;
        }

        public async Task<PessoaJuridica> BuscarPorCnpjAsync(string cnpj)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Cnpj.Equals(cnpj)) ?? new PessoaJuridica();
        }

        public async Task<PessoaJuridica> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.PessoasJuridicas.AsNoTracking().FirstOrDefaultAsync(pj => pj.Id == id) ?? new PessoaJuridica();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
