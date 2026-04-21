using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Models;
using Projeto.Moope.Pagamento.Infrastructure.Data;

namespace Projeto.Moope.Pagamento.Infrastructure.Repositories
{
    public class PagamentoReferenciaRepository : IPagamentoReferenciaRepository
    {
        private readonly AppPagamentoContext _context;

        public PagamentoReferenciaRepository(AppPagamentoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<PagamentoReferencia?> BuscarPorClienteIdAsync(Guid clienteId)
        {
            return await _context.PagamentoReferencias.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClienteId == clienteId);
        }

        public async Task<PagamentoReferencia?> BuscarPorGatewayCustomerIdAsync(string gatewayCustomerId)
        {
            return await _context.PagamentoReferencias.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GatewayCustomerId == gatewayCustomerId);
        }

        public async Task<PagamentoReferencia> SalvarAsync(PagamentoReferencia entity)
        {
            await _context.PagamentoReferencias.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PagamentoReferencia> AtualizarAsync(PagamentoReferencia entity)
        {
            _context.PagamentoReferencias.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

