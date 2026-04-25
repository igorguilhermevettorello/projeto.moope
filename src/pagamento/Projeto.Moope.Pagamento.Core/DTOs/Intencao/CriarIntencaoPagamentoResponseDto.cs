using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pagamento.Core.Enums;

namespace Projeto.Moope.Pagamento.Core.DTOs.Intencao
{
    public class CriarIntencaoPagamentoResponseDto
    {
        public Guid Id { get; init; }
        public decimal Valor { get; init; }
        public string Moeda { get; init; } = string.Empty;
        public StatusIntencaoPagamento Status { get; init; }
        public MetodoPagamento MetodoPagamento { get; init; }
        public DateTime ExpiresAt { get; init; }
    }
}
