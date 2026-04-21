using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente.GalaxPay;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.GalaxPay
{
    public interface IClienteGalaxPayCreateService
    {
        Task<ResultDto<ClienteGalaxPayDetailDto>> ExecutarAsync(ClienteGalaxPayCreateDto request, CancellationToken cancellationToken);
    }
}
