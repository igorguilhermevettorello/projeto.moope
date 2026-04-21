using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor
{
    public interface IVendedorCreateService
    {
        Task<ResultDto<VendedorCreateResultDto>> ExecutarAsync(VendedorCreateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
