using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Cliente.Infrastructure.Data;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppClienteContext _context;

        public ClienteRepository(AppClienteContext context)
        {
            _context = context;
        }

        public async Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ClienteModel?> BuscarPorIdAsync(Guid id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<IEnumerable<ClienteModel>> BuscarTodosAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<ClienteModel> SalvarAsync(ClienteModel entity)
        {
            await _context.Clientes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ClienteModel> AtualizarAsync(ClienteModel entity)
        { 
            _context.Clientes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Clientes.FindAsync(id);
            if (entity == null)
                return false;

            _context.Clientes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>()
        {
            var query = @"
                SELECT c.Id as Id,
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
                       au.LockoutEnabled as Ativo
                FROM Cliente c
                LEFT JOIN AspNetUsers au ON au.Id = c.Id
                LEFT JOIN Usuario u ON u.Id = c.Id 
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId 
                LEFT JOIN PessoaFisica pf ON pf.Id = c.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = c.Id";

            return await _context.Database.SqlQueryRaw<T>(query).ToListAsync();
        }

        public async Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id)
        {
            var query = @"
                SELECT c.Id as Id, 
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
                       e.Estado as Estado
                FROM Cliente c
                LEFT JOIN AspNetUsers au ON au.Id = c.Id
                LEFT JOIN Usuario u ON u.Id = c.Id
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId
                LEFT JOIN PessoaFisica pf ON pf.Id = c.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = c.Id
                WHERE c.Id = {0}";

            return await _context.Database.SqlQueryRaw<T>(query, id).FirstOrDefaultAsync();
        }
    }
}
