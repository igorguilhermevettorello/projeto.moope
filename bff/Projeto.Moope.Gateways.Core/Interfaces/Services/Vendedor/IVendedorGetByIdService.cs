using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor
{
    public interface IVendedorGetByIdService
    {
        Task<ResultDto<VendedorDetailDto>> ExecutarAsync(Guid vendedorId, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
