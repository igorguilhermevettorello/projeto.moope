using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Pagamento.Core.Exceptions;
using Projeto.Moope.Pagamento.Core.Interfaces.Gateways;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;
using Projeto.Moope.Pagamento.Core.Services.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Projeto.Moope.Pagamento.Core.Services
{
    public class PagamentoService : BaseService, IPagamentoService
    {
        private readonly ICelcoinPaymentGatewayClient _gatewayClient;
        private readonly IPagamentoRepository _pagamentoRepository;

        public PagamentoService(
            ICelcoinPaymentGatewayClient gatewayClient,
            IPagamentoRepository pagamentoRepository,
            INotificador notificador) : base(notificador)
        {
            _gatewayClient = gatewayClient;
            _pagamentoRepository = pagamentoRepository;
        }

        public async Task<ResultDto<GatewayTokenDto>> AutenticarGatewayAsync(string scope, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.AutenticarAsync(scope, cancellationToken);

                var accessToken = json.TryGetProperty("accessToken", out var at) ? at.GetString() : null;
                var tokenType = json.TryGetProperty("tokenType", out var tt) ? tt.GetString() : "Bearer";
                var expiresIn = json.TryGetProperty("expiresIn", out var ei) && ei.TryGetInt32(out var i) ? i : 600;
                var returnedScope = json.TryGetProperty("scope", out var sc) ? sc.GetString() : scope;

                if (string.IsNullOrWhiteSpace(accessToken))
                    return new ResultDto<GatewayTokenDto> { Status = false, Mensagem = "Token não retornado pelo gateway." };

                return new ResultDto<GatewayTokenDto>
                {
                    Status = true,
                    Dados = new GatewayTokenDto(accessToken!, expiresIn, tokenType ?? "Bearer", returnedScope ?? scope, DateTimeOffset.UtcNow)
                };
            }
            catch (Exception ex)
            {
                return FalhaToken(ex);
            }
        }

        public async Task<ResultDto<JsonElement>> CriarClienteAsync(CriarClienteGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarClienteAsync(request, cancellationToken);
                var gatewayCustomerId = TryGetFirstId(json);
                var dados = new { galaxPay = gatewayCustomerId };
                var galaxPay = JsonSerializer.SerializeToElement(dados);    
                return new ResultDto<JsonElement> { Status = true, Dados = galaxPay };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Cliente", "Erro ao criar cliente no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> ListarClientesAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.ListarClientesAsync(query, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Cliente", "Erro ao listar clientes no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> CriarPlanoAsync(CriarPlanoGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarPlanoAsync(request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Plano", "Erro ao criar plano no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> ListarPlanosAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.ListarPlanosAsync(query, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Plano", "Erro ao listar planos no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> CriarCartaoAsync(CriarCartaoGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarCartaoAsync(request.CustomerId, request.TypeId, request.Payload, cancellationToken);
                var gatewayCardId = TryGetFirstId(json);
                var dados = new { galaxPay = gatewayCardId };
                var galaxPay = JsonSerializer.SerializeToElement(dados);
                return new ResultDto<JsonElement> { Status = true, Dados = galaxPay };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Cartao", "Erro ao criar cartão no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> CriarAssinaturaComPlanoAsync(CriarAssinaturaComPlanoGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarAssinaturaAsync(request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Assinatura", "Erro ao criar assinatura/contrato no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> CriarAssinaturaSemPlanoAsync(CriarAssinaturaSemPlanoGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarAssinaturaAsync(request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Assinatura", "Erro ao criar assinatura/contrato no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> CriarAssinaturaManualAsync(CriarAssinaturaManualGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarAssinaturaManualAsync(request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Assinatura", "Erro ao criar assinatura/contrato manual no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> AdicionarTransacaoEmAssinaturaAsync(AdicionarTransacaoEmAssinaturaGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.AdicionarTransacaoEmAssinaturaAsync(request.SubscriptionId, request.TypeId, request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Transacao", "Erro ao adicionar transação em assinatura no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> ListarTransacoesAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.ListarTransacoesAsync(query, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Transacao", "Erro ao listar transações no gateway.");
            }
        }

        public async Task<ResultDto> CancelarTransacaoAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return new ResultDto { Status = false, Mensagem = "transactionId é obrigatório." };

                _ = await _gatewayClient.CancelarTransacaoAsync(transactionId, "galaxPayId", cancellationToken);
                return new ResultDto { Status = true };
            }
            catch (Exception ex)
            {
                var result = FalhaGateway(ex, "Transacao", "Erro ao cancelar transação no gateway.");
                return new ResultDto { Status = result.Status, Mensagem = result.Mensagem };
            }
        }

        public async Task<ResultDto<JsonElement>> CriarCobrancaAvulsaAsync(CriarCobrancaAvulsaGatewayRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.CriarCobrancaAvulsaAsync(request.Payload, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Cobranca", "Erro ao criar cobrança avulsa no gateway.");
            }
        }

        public async Task<ResultDto<JsonElement>> ListarCobrancasAvulsasAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await _gatewayClient.ListarCobrancasAvulsasAsync(query, cancellationToken);
                return new ResultDto<JsonElement> { Status = true, Dados = json };
            }
            catch (Exception ex)
            {
                return FalhaGateway(ex, "Cobranca", "Erro ao listar cobranças avulsas no gateway.");
            }
        }

        private ResultDto<JsonElement> FalhaGateway(Exception ex, string campo, string mensagemPadrao)
        {
            var msg = mensagemPadrao;

            if (ex is CelcoinGatewayException gwEx)
            {
                msg = $"{mensagemPadrao} Status: {(int)gwEx.StatusCode}.";
                if (!string.IsNullOrWhiteSpace(gwEx.ResponseBody))
                    msg += $" Body: {gwEx.ResponseBody}";
            }
            else if (ex is HttpRequestException)
            {
                msg = $"{mensagemPadrao} Erro de conexão: {ex.Message}";
            }
            else if (ex is TaskCanceledException)
            {
                msg = $"{mensagemPadrao} Timeout/Cancelamento: {ex.Message}";
            }
            else
            {
                msg = $"{mensagemPadrao} {ex.Message}";
            }

            Notificar(campo, msg);
            return new ResultDto<JsonElement> { Status = false, Mensagem = msg };
        }

        private ResultDto<GatewayTokenDto> FalhaToken(Exception ex)
        {
            var msg = ex is CelcoinGatewayException gwEx
                ? $"Erro ao autenticar no gateway. Status: {(int)gwEx.StatusCode}. Body: {gwEx.ResponseBody}"
                : $"Erro ao autenticar no gateway. {ex.Message}";

            Notificar("Autenticacao", msg);
            return new ResultDto<GatewayTokenDto> { Status = false, Mensagem = msg };
        }

        private static string? TryGetFirstId(JsonElement json)
        {
            // Extrator genérico: percorre o JSON (objetos/arrays) buscando um ID em campos comuns
            // (ex.: galaxPayId) sem acoplar no schema de cada endpoint.
            var candidates = new[]
            {
                "galaxPayId",
                "id",
                "customerId"
            };

            return TryFindFirstPropertyValue(json, candidates, maxDepth: 10);
        }

        private static string? TryFindFirstPropertyValue(JsonElement element, IReadOnlyList<string> candidatePropertyNames, int maxDepth)
        {
            if (maxDepth < 0)
                return null;

            static string? AsNonEmptyString(JsonElement value)
            {
                if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    return null;

                var s = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
                return string.IsNullOrWhiteSpace(s) ? null : s;
            }

            static bool TryGetPropertyIgnoreCase(JsonElement obj, string name, out JsonElement value)
            {
                foreach (var prop in obj.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = prop.Value;
                        return true;
                    }
                }

                value = default;
                return false;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                // Prioriza encontrar o ID no nível atual (envelope ou entidade)
                foreach (var candidate in candidatePropertyNames)
                {
                    if (TryGetPropertyIgnoreCase(element, candidate, out var value))
                    {
                        var s = AsNonEmptyString(value);
                        if (!string.IsNullOrWhiteSpace(s))
                            return s;
                    }
                }

                // Depois, desce recursivamente em todas as propriedades (ex.: Customer, Card, etc.)
                foreach (var prop in element.EnumerateObject())
                {
                    var nested = TryFindFirstPropertyValue(prop.Value, candidatePropertyNames, maxDepth - 1);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = TryFindFirstPropertyValue(item, candidatePropertyNames, maxDepth - 1);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }

            return null;
        }
    }
}

