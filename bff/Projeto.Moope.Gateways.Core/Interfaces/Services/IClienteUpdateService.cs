using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IClienteUpdateService
    {
        Task<ResultDto<ClienteDetailDto>> ExecutarAsync(ClienteUpdateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}

