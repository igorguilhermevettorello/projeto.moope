using Projeto.Moope.Core.Interfaces.Data;
using PagamentoModel = Projeto.Moope.Pagamento.Core.Models.Pagamento;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Repositories
{
    public interface IPagamentoRepository : IDisposable
    {
        IUnitOfWork UnitOfWork { get; }

        Task<PagamentoModel?> BuscarPorClienteIdAsync(Guid clienteId);
        Task<PagamentoModel?> BuscarPorGatewayCustomerIdAsync(string gatewayCustomerId);

        Task<PagamentoModel> SalvarAsync(PagamentoModel entity);
        Task<PagamentoModel> AtualizarAsync(PagamentoModel entity);
    }
}

