using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cartao;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.GalaxPay
{
    public interface ICartaoGalaxPayCreateService
    {
        Task<ResultDto<CartaoGalaxPayDetailDto>> ExecutarAsync(CartaoGalaxPayCreateDto request, CancellationToken cancellationToken);
    }
}
