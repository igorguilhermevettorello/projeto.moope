using Microsoft.Extensions.Logging;
using Projeto.Moope.Pagamento.Core.DTOs.Idempotencia;
using Projeto.Moope.Pagamento.Core.Enums;
using Projeto.Moope.Pagamento.Core.Excecoes.Idempotencia;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;
using IdempotenciaModel = Projeto.Moope.Pagamento.Core.Models.IdempotenciaPagamento;

namespace Projeto.Moope.Pagamento.Core.Services
{
    public class IdempotenciaService : IIdempotenciaService
    {
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly ILogger<IdempotenciaService> _logger;

        public IdempotenciaService(IIdempotenciaRepository idempotenciaRepository, ILogger<IdempotenciaService> logger)
        {
            _idempotenciaRepository = idempotenciaRepository;
            _logger = logger;
        }

        public async Task<ResultadoInicioIdempotenciaDto> IniciarProcessamentoAsync(
            string idempotencyKey,
            string scope,
            string requestHash,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("IdempotencyKey é obrigatório.", nameof(idempotencyKey));
            if (string.IsNullOrWhiteSpace(scope))
                throw new ArgumentException("Scope é obrigatório.", nameof(scope));
            if (string.IsNullOrWhiteSpace(requestHash))
                throw new ArgumentException("RequestHash é obrigatório.", nameof(requestHash));

            var existente = await _idempotenciaRepository.ObterPorChaveEscopoAsync(idempotencyKey, scope, cancellationToken);
            if (existente == null)
            {
                var novo = new IdempotenciaModel
                {
                    Id = Guid.NewGuid(),
                    IdempotencyKey = idempotencyKey.Trim(),
                    Scope = scope.Trim(),
                    RequestHash = requestHash,
                    Status = StatusIdempotencia.EmProcessamento,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                try
                {
                    await _idempotenciaRepository.AdicionarAsync(novo, cancellationToken);
                    return new ResultadoInicioIdempotenciaDto
                    {
                        DeveProcessar = true,
                        JaConcluido = false,
                        EmProcessamento = true,
                        Status = novo.Status,
                        IdempotenciaId = novo.Id
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Concorrência ao inserir idempotência (Key={Key}, Scope={Scope}). Tentando recuperar existente.", idempotencyKey, scope);
                }

                existente = await _idempotenciaRepository.ObterPorChaveEscopoAsync(idempotencyKey, scope, cancellationToken);
                if (existente == null)
                    throw new InvalidOperationException("Não foi possível criar nem recuperar o registro de idempotência.");
            }

            if (!string.Equals(existente.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new ChaveIdempotenteReutilizadaComPayloadDiferenteException(
                    "Chave de idempotência reutilizada com payload diferente para o mesmo escopo.");
            }

            if (existente.Status == StatusIdempotencia.Concluido)
            {
                return new ResultadoInicioIdempotenciaDto
                {
                    DeveProcessar = false,
                    JaConcluido = true,
                    EmProcessamento = false,
                    Status = existente.Status,
                    IdempotenciaId = existente.Id,
                    ResponseStatusCode = existente.ResponseStatusCode,
                    ResponseBody = existente.ResponseBody,
                    ResourceId = existente.ResourceId,
                    ResourceType = existente.ResourceType
                };
            }

            if (existente.Status == StatusIdempotencia.EmProcessamento)
            {
                return new ResultadoInicioIdempotenciaDto
                {
                    DeveProcessar = false,
                    JaConcluido = false,
                    EmProcessamento = true,
                    Status = existente.Status,
                    IdempotenciaId = existente.Id
                };
            }

            if (existente.Status == StatusIdempotencia.Falhou)
            {
                existente.Status = StatusIdempotencia.EmProcessamento;
                existente.UpdatedAt = DateTime.UtcNow;
                existente.ResponseStatusCode = null;
                existente.ResponseBody = null;
                existente.ResourceId = null;
                existente.ResourceType = null;

                await _idempotenciaRepository.AtualizarAsync(existente, cancellationToken);

                return new ResultadoInicioIdempotenciaDto
                {
                    DeveProcessar = true,
                    JaConcluido = false,
                    EmProcessamento = true,
                    Status = existente.Status,
                    IdempotenciaId = existente.Id
                };
            }

            throw new InconsistenciaEstadoIdempotenciaException("Status de idempotência inválido.");
        }

        public async Task ConcluirAsync(
            Guid idempotenciaId,
            int responseStatusCode,
            string responseBody,
            string? resourceId,
            string? resourceType,
            CancellationToken cancellationToken)
        {
            if (idempotenciaId == Guid.Empty)
                throw new ArgumentException("IdempotenciaId é obrigatório.", nameof(idempotenciaId));
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("ResponseBody é obrigatório.", nameof(responseBody));

            var registro = await _idempotenciaRepository.BuscarPorIdAsync(idempotenciaId);
            if (registro == null)
                throw new RegistroIdempotenciaNaoEncontradoException("Não foi possível concluir: registro de idempotência não encontrado.");

            if (registro.Status != StatusIdempotencia.EmProcessamento)
                throw new InconsistenciaEstadoIdempotenciaException("Não foi possível concluir: o registro não está em processamento.");

            registro.Status = StatusIdempotencia.Concluido;
            registro.ResponseStatusCode = responseStatusCode;
            registro.ResponseBody = responseBody;
            registro.ResourceId = resourceId;
            registro.ResourceType = resourceType;
            registro.UpdatedAt = DateTime.UtcNow;

            await _idempotenciaRepository.AtualizarAsync(registro, cancellationToken);
        }

        public async Task MarcarFalhaAsync(Guid idempotenciaId, CancellationToken cancellationToken)
        {
            if (idempotenciaId == Guid.Empty)
                throw new ArgumentException("IdempotenciaId é obrigatório.", nameof(idempotenciaId));

            var registro = await _idempotenciaRepository.BuscarPorIdAsync(idempotenciaId);
            if (registro == null)
                throw new RegistroIdempotenciaNaoEncontradoException("Não foi possível marcar falha: registro de idempotência não encontrado.");

            if (registro.Status != StatusIdempotencia.EmProcessamento)
                throw new InconsistenciaEstadoIdempotenciaException("Não foi possível marcar falha: o registro não está em processamento.");

            registro.Status = StatusIdempotencia.Falhou;
            registro.UpdatedAt = DateTime.UtcNow;

            await _idempotenciaRepository.AtualizarAsync(registro, cancellationToken);
        }
    }
}

