using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IClienteGetByIdService
    {
        Task<ResultDto<ClienteDetailDto>> ExecutarAsync(Guid clienteId, string? authorizationHeader, CancellationToken cancellationToken);
    }
}

