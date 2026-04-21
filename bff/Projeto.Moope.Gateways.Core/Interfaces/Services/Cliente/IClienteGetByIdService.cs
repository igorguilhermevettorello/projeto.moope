using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente
{
    public interface IClienteGetByIdService
    {
        Task<ResultDto<ClienteDetailDto>> ExecutarAsync(Guid clienteId, string? authorizationHeader, CancellationToken cancellationToken);
    }
}

