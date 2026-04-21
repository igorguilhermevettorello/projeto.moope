using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente
{
    public interface IClienteGalaxPayUpdateService
    {
        
        Task<ResultDto<ClienteDetailDto>> ExecutarAsync(ClienteGalaxPayUpdateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
