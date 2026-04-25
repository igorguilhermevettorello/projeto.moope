using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Pedido;

namespace Projeto.Moope.RabbitMQ.Core.Interfaces.Services
{
    public interface IPedidoValoresPagamentoQueryService
    {
        Task<ResultDto<PedidoValoresPagamentoDto>> ObterValoresPagamentoAsync(
            Guid pedidoId,
            string authorizationHeader,
            CancellationToken cancellationToken);
    }
}

