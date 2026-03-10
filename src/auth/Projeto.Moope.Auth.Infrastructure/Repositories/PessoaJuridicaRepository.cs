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

        public IUnitOfWork UnitOfWork => throw new NotImplementedException();

        public PessoaJuridicaRepository(AppAuthContext context)
        {
            _context = context;
        }

        public async Task<PessoaJuridica> BuscarPorIdAsync(Guid id)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Id == id);
        }

        public async Task<IEnumerable<PessoaJuridica>> BuscarTodosAsync()
        {
            return await _context.PessoasJuridicas.ToListAsync();
        }

        public async Task<PessoaJuridica> SalvarAsync(PessoaJuridica entity)
        {
            await _context.PessoasJuridicas.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PessoaJuridica> AtualizarAsync(PessoaJuridica entity)
        {
            _context.PessoasJuridicas.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pessoaJuridica = await _context.PessoasJuridicas.FindAsync(id);
            if (pessoaJuridica != null)
            {
                _context.PessoasJuridicas.Remove(pessoaJuridica);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<PessoaJuridica> BuscarPorCnpjAsync(string cnpj)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Cnpj.Equals(cnpj));
        }

        public async Task<PessoaJuridica> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.PessoasJuridicas.AsNoTracking().FirstOrDefaultAsync(pj => pj.Id == id);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
