using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Endereco.Core.Interfaces.Repositories;
using Projeto.Moope.Endereco.Infrastructure.Data;
using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Infrastructure.Repositories
{
    public class EnderecoRepository : IEnderecoRepository
    {
        private readonly AppEnderecoContext _context;

        public EnderecoRepository(AppEnderecoContext context)
        {
            _context = context;
        }

        public async Task<EnderecoModel?> GetById(Guid id)
        {
            return await _context.Enderecos.FindAsync(id);
        }

        public async Task<IEnumerable<EnderecoModel>> GetAll()
        {
            return await _context.Enderecos.ToListAsync();
        }

        public async Task Add(EnderecoModel endereco)
        {
            await _context.Enderecos.AddAsync(endereco);
            await _context.SaveChangesAsync();
        }

        public async Task Update(EnderecoModel endereco)
        {
            _context.Enderecos.Update(endereco);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var endereco = await GetById(id);
            if (endereco != null)
            {
                _context.Enderecos.Remove(endereco);
                await _context.SaveChangesAsync();
            }
        }
    }
}