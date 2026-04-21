using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Venda;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.RabbitMQ
{
    public interface IVendaSendQueueService
    {
        Task<ResultDto> ExecutarAsync(VendaQueueDto request, CancellationToken cancellationToken);
    }
}
