using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Infrastructure.Data;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppPedidoContext _context;

        public PedidoRepository(AppPedidoContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<PedidoModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Pedidos.FindAsync(id))!;
        }

        public async Task<PedidoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PedidoModel?> BuscarPorIdComTransacoesEDescontoAsync(Guid id)
        {
            return await _context.Pedidos
                .Include(p => p.Transacoes)
                .Include(p => p.Desconto)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<PedidoModel>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .OrderByDescending(p => p.Created)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PedidoModel>> BuscarTodosAsync()
        {
            return await _context.Pedidos.ToListAsync();
        }

        public async Task<PedidoModel> SalvarAsync(PedidoModel entity)
        {
            await _context.Pedidos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PedidoModel> AtualizarAsync(PedidoModel entity)
        {
            _context.Pedidos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Pedidos.FindAsync(id);
            if (entity == null)
                return false;

            _context.Pedidos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TransacaoModel>> BuscarTransacoesPorPedidoIdAsync(Guid pedidoId)
        {
            return await _context.Transacoes
                .AsNoTracking()
                .Where(t => t.PedidoId == pedidoId)
                .ToListAsync();
        }

        public async Task<bool> RemoverTransacoesPorPedidoIdAsync(Guid pedidoId)
        {
            var transacoes = await _context.Transacoes.Where(t => t.PedidoId == pedidoId).ToListAsync();
            if (transacoes.Count == 0)
                return true;

            _context.Transacoes.RemoveRange(transacoes);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TransacaoModel>> SalvarTransacoesAsync(IEnumerable<TransacaoModel> transacoes)
        {
            var lista = transacoes.ToList();
            await _context.Transacoes.AddRangeAsync(lista);
            await _context.SaveChangesAsync();
            return lista;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

