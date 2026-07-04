using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido
{
    public interface IPedidoListService
    {
        Task<ResultDto<IEnumerable<PedidoListItemDto>>> ExecutarAsync(string? authorizationHeader, CancellationToken cancellationToken);
    }
}
