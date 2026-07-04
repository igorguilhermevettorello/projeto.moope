using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Pedido.Core.DTOs.Pedido;

namespace Projeto.Moope.Pedido.Core.Interfaces.Services
{
    public interface IPedidoListService
    {
        Task<ResultDto<IReadOnlyList<PedidoListItemDto>>> ListarAsync(CancellationToken cancellationToken = default);
    }
}
