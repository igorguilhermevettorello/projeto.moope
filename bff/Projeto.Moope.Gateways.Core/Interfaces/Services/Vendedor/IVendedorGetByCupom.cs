using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor
{
    public interface IVendedorGetByCupom
    {
        Task<ResultDto<VendedorDetailDto>> ExecutarAsync(string cupom, string? authorizationHeader, CancellationToken cancellationToken);
    }
}
