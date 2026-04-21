using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Plano;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IPlanoGetById
    {
        Task<ResultDto<PlanoDetailDto>> ExecutarAsync(Guid planoId, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
