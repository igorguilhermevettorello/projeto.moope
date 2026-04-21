using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Venda;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IProcessarVendaService
    {
        Task<ResultDto<VendaProcessingDto>> ExecutarAsync(
            VendaCreateDto request,
            VendaUsuarioDto? usuario,
            string? authorizationHeader,
            string? idempotencyKey,
            CancellationToken cancellationToken);
    }
}
