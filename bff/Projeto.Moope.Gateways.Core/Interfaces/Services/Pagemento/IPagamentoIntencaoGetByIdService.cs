using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services.Pagemento
{
    public interface IPagamentoIntencaoGetByIdService
    {
        Task<ResultDto> ExecutarAsync(Guid idempotencyKey, string idempotencyKeyHeader, CancellationToken cancellationToken);
    }
}

