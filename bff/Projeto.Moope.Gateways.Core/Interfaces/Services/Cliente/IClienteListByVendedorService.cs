using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente
{
    public interface IClienteListByVendedorService
    {
        Task<ResultDto<IEnumerable<ClienteListItemDto>>> ExecutarAsync(string? authorizationHeader, CancellationToken cancellationToken);
    }
}
