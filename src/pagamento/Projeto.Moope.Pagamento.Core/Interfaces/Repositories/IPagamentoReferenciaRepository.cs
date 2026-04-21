using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pagamento.Core.Models;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Repositories
{
    public interface IPagamentoReferenciaRepository : IDisposable
    {
        IUnitOfWork UnitOfWork { get; }

        Task<PagamentoReferencia?> BuscarPorClienteIdAsync(Guid clienteId);
        Task<PagamentoReferencia?> BuscarPorGatewayCustomerIdAsync(string gatewayCustomerId);

        Task<PagamentoReferencia> SalvarAsync(PagamentoReferencia entity);
        Task<PagamentoReferencia> AtualizarAsync(PagamentoReferencia entity);
    }
}

