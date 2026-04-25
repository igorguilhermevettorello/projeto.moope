using Projeto.Moope.Pagamento.Core.DTOs.Intencao;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Services
{
    public interface IIntencaoPagamentoService
    {
        Task<IdempotencyKeyDto?> CriarAsync(
            CriarIntencaoPagamentoRequestDto requisicao,
            CancellationToken cancellationToken = default);

        Task<CriarIntencaoPagamentoResponseDto?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
