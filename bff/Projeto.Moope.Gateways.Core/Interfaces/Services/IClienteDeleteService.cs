using Projeto.Moope.Gateways.Core.DTOs;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IClienteDeleteService
    {
        Task<ResultDto> ExecutarAsync(Guid clienteId, string? authorizationHeader, CancellationToken cancellationToken);
    }
}

