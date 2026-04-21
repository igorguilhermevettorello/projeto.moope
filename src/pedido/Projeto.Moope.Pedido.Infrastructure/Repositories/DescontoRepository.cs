using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Models;
using Projeto.Moope.Pedido.Infrastructure.Data;

namespace Projeto.Moope.Pedido.Infrastructure.Repositories
{
    public class DescontoRepository : IDescontoRepository
    {
        private readonly AppPedidoContext _context;

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public DescontoRepository(AppPedidoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Desconto>> BuscarPorPedidoIdAsync(Guid pedidoId)
        {
            return await _context.Descontos
                .Where(d => d.PedidoId == pedidoId && d.Ativo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Desconto>> BuscarPorCodigoDescontoAsync(string codigoDesconto)
        {
            return await _context.Descontos
                .Where(d => d.CodigoDesconto == codigoDesconto && d.Ativo)
                .ToListAsync();
        }

        public async Task<Desconto?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Descontos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public Task<Desconto> BuscarPorIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Desconto>> BuscarTodosAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Desconto> SalvarAsync(Desconto entity)
        {
            throw new NotImplementedException();
        }

        public Task<Desconto> AtualizarAsync(Desconto entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoverAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
