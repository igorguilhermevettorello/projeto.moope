using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Pagamento.Core.Configurations;
using Projeto.Moope.Pagamento.Core.DTOs.Intencao;
using Projeto.Moope.Pagamento.Core.Enums;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;
using IntencaoModel = Projeto.Moope.Pagamento.Core.Models.IntencaoPagamento;

namespace Projeto.Moope.Pagamento.Core.Services
{
    public class IntencaoPagamentoService : BaseService, IIntencaoPagamentoService
    {
        private readonly IIntencaoPagamentoRepository _repositorio;
        private readonly IntencaoPagamentoOptions _opcoes;
        private readonly ILogger<IntencaoPagamentoService> _logger;

        public IntencaoPagamentoService(
            IIntencaoPagamentoRepository repositorio,
            IOptions<IntencaoPagamentoOptions> opcoes,
            INotificador notificador,
            ILogger<IntencaoPagamentoService> logger)
            : base(notificador)
        {
            _repositorio = repositorio;
            _opcoes = opcoes.Value;
            _logger = logger;
        }

        public async Task<IdempotencyKeyDto?> CriarAsync(
            CriarIntencaoPagamentoRequestDto requisicao,
            CancellationToken cancellationToken = default)
        {
            if (requisicao.Valor <= 0)
            {
                Notificar(nameof(requisicao.Valor), "Valor deve ser maior que zero.");
                return null;
            }

            var moedaNormalizada = requisicao.Moeda?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(moedaNormalizada) || moedaNormalizada.Length != 3)
            {
                Notificar(nameof(requisicao.Moeda), "Moeda deve ser informada com 3 caracteres (ex.: BRL).");
                return null;
            }

            if (!Enum.IsDefined(typeof(MetodoPagamento), requisicao.MetodoPagamento))
            {
                Notificar(nameof(requisicao.MetodoPagamento), "Método de pagamento inválido.");
                return null;
            }

            var minutosValidade = _opcoes.MinutosValidade < 1 ? 30 : _opcoes.MinutosValidade;
            var agora = DateTime.UtcNow;

            var intencao = new IntencaoModel
            {
                Valor = requisicao.Valor,
                Moeda = moedaNormalizada.ToUpperInvariant(),
                Status = StatusIntencaoPagamento.Criada,
                MetodoPagamento = requisicao.MetodoPagamento,
                ExpiresAt = agora.AddMinutes(minutosValidade),
                CreatedAt = agora,
                UpdatedAt = agora
            };

            var salva = await _repositorio.AdicionarAsync(intencao, cancellationToken);

            _logger.LogInformation(
                "Intenção de pagamento criada. IntencaoPagamentoId={IntencaoPagamentoId} MetodoPagamento={MetodoPagamento}",
                salva.Id,
                salva.MetodoPagamento);


            return new IdempotencyKeyDto { IdempotencyKey = salva.Id };
        }

        public async Task<CriarIntencaoPagamentoResponseDto?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entidade = await _repositorio.ObterPorIdAsync(id, cancellationToken);
            return entidade is null ? null : MapearResposta(entidade);
        }

        private static CriarIntencaoPagamentoResponseDto MapearResposta(IntencaoModel entidade)
        {
            return new CriarIntencaoPagamentoResponseDto
            {
                Id = entidade.Id,
                Valor = entidade.Valor,
                Moeda = entidade.Moeda,
                Status = entidade.Status,
                MetodoPagamento = entidade.MetodoPagamento,
                ExpiresAt = entidade.ExpiresAt
            };
        }
    }
}
