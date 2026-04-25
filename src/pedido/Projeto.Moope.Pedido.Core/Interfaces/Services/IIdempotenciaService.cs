using Projeto.Moope.Pedido.Core.DTOs.Idempotencia;

namespace Projeto.Moope.Pedido.Core.Interfaces.Services
{
    public interface IIdempotenciaService
    {
        Task<ResultadoInicioIdempotenciaDto> IniciarProcessamentoAsync(
            string idempotencyKey,
            string scope,
            string requestHash,
            CancellationToken cancellationToken);

        Task ConcluirAsync(
            Guid idempotenciaId,
            int responseStatusCode,
            string responseBody,
            string? resourceId,
            string? resourceType,
            CancellationToken cancellationToken);

        Task MarcarFalhaAsync(
            Guid idempotenciaId,
            CancellationToken cancellationToken);
    }
}

