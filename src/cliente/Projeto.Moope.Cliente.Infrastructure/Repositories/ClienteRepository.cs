using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Cliente.Infrastructure.Data;

namespace Projeto.Moope.Cliente.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppClienteContext _context;

        public ClienteRepository(AppClienteContext context)
        {
            _context = context;
        }

        public async Task Add(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }
    }
}