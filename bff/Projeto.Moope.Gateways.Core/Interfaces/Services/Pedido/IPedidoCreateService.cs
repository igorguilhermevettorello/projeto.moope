using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido
{
    public interface IPedidoCreateService
    {
        Task<ResultDto<PedidoDetailDto>> ExecutarAsync(PedidoCreateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
