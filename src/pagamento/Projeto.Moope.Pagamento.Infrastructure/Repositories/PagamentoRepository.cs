using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Infrastructure.Data;
using PagamentoModel = Projeto.Moope.Pagamento.Core.Models.Pagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly AppPagamentoContext _context;

        public PagamentoRepository(AppPagamentoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<PagamentoModel?> BuscarPorClienteIdAsync(Guid clienteId)
        {
            return await _context.Pagamentos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClienteId == clienteId);
        }

        public async Task<PagamentoModel?> BuscarPorGatewayCustomerIdAsync(string gatewayCustomerId)
        {
            return await _context.Pagamentos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GatewayCustomerId == gatewayCustomerId);
        }

        public async Task<PagamentoModel> SalvarAsync(PagamentoModel entity)
        {
            await _context.Pagamentos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PagamentoModel> AtualizarAsync(PagamentoModel entity)
        {
            _context.Pagamentos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

