using Projeto.Moope.Pedido.Core.DTOs.Plano;

namespace Projeto.Moope.Pedido.Core.Interfaces.Gateways
{
    public interface IPlanoReadGateway
    {
        Task<PlanoDetailDto?> ObterPorIdAsync(Guid planoId, CancellationToken cancellationToken = default);
    }
}
