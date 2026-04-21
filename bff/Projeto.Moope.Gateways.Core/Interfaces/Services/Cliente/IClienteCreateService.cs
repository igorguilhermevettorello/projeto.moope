using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente
{
    public interface IClienteCreateService
    {
        Task<ResultDto<ClienteCreateResultDto>> ExecutarAsync(ClienteCreateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
