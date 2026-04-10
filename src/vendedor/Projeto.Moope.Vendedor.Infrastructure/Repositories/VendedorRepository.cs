using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Vendedor.Core.Interfaces.Repositories;
using Projeto.Moope.Vendedor.Infrastructure.Data;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Infrastructure.Repositories
{
    public class VendedorRepository : IVendedorRepository
    {
        private readonly AppVendedorContext _context;

        public VendedorRepository(AppVendedorContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<VendedorModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Vendedores.FindAsync(id))!;
        }

        public async Task<VendedorModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<VendedorModel?> BuscarPorCodigoCupomAsync(string codigoCupom)
        {
            return await _context.Vendedores.AsNoTracking()
                .FirstOrDefaultAsync(v => v.CodigoCupom == codigoCupom);
        }

        public async Task<IEnumerable<VendedorModel>> BuscarTodosAsync()
        {
            return await _context.Vendedores.ToListAsync();
        }

        public async Task<VendedorModel> SalvarAsync(VendedorModel entity)
        {
            await _context.Vendedores.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<VendedorModel> AtualizarAsync(VendedorModel entity)
        {
            _context.Vendedores.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Vendedores.FindAsync(id);
            if (entity == null)
                return false;

            _context.Vendedores.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        //public async Task<IEnumerable<VendedorModel>> BuscarTodosComDadosRelacionadosAsync()
        //{
        //    return await _context.Vendedores
        //        .Include(v => v.Usuario)
        //            .ThenInclude(u => u.Endereco)
        //        .Include(v => v.Usuario)
        //            .ThenInclude(u => u.PessoaFisica)
        //        .Include(v => v.Usuario)
        //            .ThenInclude(u => u.PessoaJuridica)
        //        .Include(v => v.Usuario)
        //            .ThenInclude(u => u.IdentityUser)
        //        .AsNoTracking()
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>()
        {
            var query = @"
                SELECT v.Id as Id,
                       u.Nome as Nome, 
                       au.Email as Email, 
                       pf.Cpf, 
                       pj.Cnpj,
                       CASE 
                           WHEN pf.Cpf IS NOT NULL THEN '1'
                           WHEN pj.Cnpj IS NOT NULL THEN '2'
                           ELSE NULL
                       END as TipoPessoa,
                       COALESCE(pf.Cpf, pj.Cnpj) as CpfCnpj,
                       au.PhoneNumber as Telefone, 
                       e.Cidade as Cidade, 
                       e.Estado as Estado, 
                       au.LockoutEnabled as Ativo,
                       v.CodigoCupom as CodigoCupom
                FROM Vendedor v
                LEFT JOIN AspNetUsers au ON au.Id = v.Id
                LEFT JOIN Usuario u ON u.Id = v.Id 
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId 
                LEFT JOIN PessoaFisica pf ON pf.Id = v.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = v.Id";

            return await _context.Database.SqlQueryRaw<T>(query).ToListAsync();
        }

        public async Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id)
        {
            var query = @"
                SELECT v.Id as Id, 
                       u.Nome as Nome, 
                       au.Email as Email, 
                       CASE 
                         WHEN pf.Cpf IS NOT NULL THEN '1'
                         WHEN pj.Cnpj IS NOT NULL THEN '2'
                         ELSE NULL
                       END as TipoPessoa,
                       COALESCE(pf.Cpf, pj.Cnpj) as CpfCnpj,
                       au.PhoneNumber as Telefone, 
                       au.LockoutEnabled as Ativo,
                       e.Cep as Cep, 
                       e.Logradouro as Logradouro,
                       e.Numero as Numero,
                       e.Complemento as Complemento,
                       e.Bairro as Bairro,
                       e.Cidade as Cidade,
                       e.Estado as Estado,
                       v.ChavePix as ChavePix,
                       v.PercentualComissao as PercentualComissao,
                       v.CodigoCupom as CodigoCupom,
                       pj.NomeFantasia as NomeFantasia,
                       pj.InscricaoEstadual as InscricaoEstadual
                FROM Vendedor v
                LEFT JOIN AspNetUsers au ON au.Id = v.Id
                LEFT JOIN Usuario u ON u.Id = v.Id
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId
                LEFT JOIN PessoaFisica pf ON pf.Id = v.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = v.Id
                WHERE v.Id = {0}";

            return await _context.Database.SqlQueryRaw<T>(query, id).FirstOrDefaultAsync();


        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
