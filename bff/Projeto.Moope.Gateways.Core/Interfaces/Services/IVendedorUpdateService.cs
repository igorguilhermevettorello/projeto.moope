using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IVendedorUpdateService
    {
        Task<ResultDto<VendedorDetailDto>> ExecutarAsync(VendedorUpdateDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
