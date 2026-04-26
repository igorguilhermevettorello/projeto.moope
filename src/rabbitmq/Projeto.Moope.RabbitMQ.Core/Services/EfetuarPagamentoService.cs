using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.Helpers;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;

namespace Projeto.Moope.RabbitMQ.Core.Services
{
    public class EfetuarPagamentoService : IEfetuarPagamentoService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly DownstreamApisOptions apis;
        private readonly ILogger<EfetuarPagamentoService> logger;

        public EfetuarPagamentoService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<EfetuarPagamentoService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.apis = apis.Value;
            this.logger = logger;
        }

        public async Task<ResultDto> EfetuarPagamento(PagamentoDto request, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apis.Pagamento))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:Pagamento nao configurado."
                };
            }

            var endpoint = "/api/pagamento/assinaturas/sem-plano";
            if (request.TaxaAdesao > 0)
            {
                endpoint = "/api/pagamento/assinaturas/sem-plano-com-taxa";
            }

            //var endpoint = "/api/pagamento/assinaturas/sem-plano";
            var url = UrlHelper.Combine(apis.Pagamento, endpoint);
            var payload = new 
            {
                MyId = request.PedidoId,
                Value = (int)(request.Valor * 100),
                Quantity = 0,
                Periodicity = request.Periodicidade.ToString(),
                FirstPayDayDate = DateTime.Now.ToString("yyyy-MM-dd"),
                MainPaymentMethodId = request.MetodoPagamento.ToString(),
                AdditionalInfo = request.Observacao,
                Customer = new { 
                    GalaxPayId = request.GalaxPayCustomerId.ToString(),
                    request.Name,
                    Emails = new List<string> { request.Email }
                },
                Card = new { GalaxPayId = request.GalaxPayCardId.ToString() },
            };

            var body = new {
                request.Name,
                request.Email,
                request.ClienteId,
                request.PedidoId,
                request.Valor,
                request.Periodicidade,
                request.MetodoPagamento,
                request.GalaxPayCustomerId ,
                request.GalaxPayCardId,
                request.Observacao,
                request.TaxaAdesao,
                payload 
            };

            try
            {
                var httpClient = httpClientFactory.CreateClient();

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body, options: JsonOptions)
                };

                if (!string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
                }

                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Idempotency-Key", request.IdempotencyKey);
                }

                using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Falha ao efetuar pagamento. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseBody);

                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = string.IsNullOrWhiteSpace(responseBody)
                            ? "Falha ao efetuar pagamento no servico Pagamento."
                            : responseBody
                    };
                }

                var galaxPayId = TryExtractGalaxPayIdAsInt(responseBody);
                if (!galaxPayId.HasValue || galaxPayId.Value <= 0)
                {
                    logger.LogWarning(
                        "Resposta inesperada do servico Pagamento (galaxPayId ausente/invalido). PedidoId: {PedidoId}. Body: {Body}",
                        request.PedidoId,
                        responseBody);

                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = 502,
                        Mensagem = "Resposta inesperada do servico Pagamento: galaxPayId nao retornado."
                    };
                }

                if (string.IsNullOrWhiteSpace(apis.Pedido))
                {
                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = 500,
                        Mensagem = "DownstreamApis:Pedido nao configurado."
                    };
                }

                if (string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = 500,
                        Mensagem = "Authorization header nao informado."
                    };
                }

                var patchUrl = UrlHelper.Combine(apis.Pedido, $"/api/pedido/{request.PedidoId}/galaxpayid/{galaxPayId.Value}");
                using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);
                patchRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

                using var patchResponse = await httpClient.SendAsync(patchRequest, cancellationToken);
                var patchResponseBody = await patchResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!patchResponse.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Falha ao atualizar GalaxPayId no servico Pedido. PedidoId: {PedidoId}. GalaxPayId: {GalaxPayId}. Status: {StatusCode}. Body: {Body}",
                        request.PedidoId,
                        galaxPayId.Value,
                        (int)patchResponse.StatusCode,
                        patchResponseBody);

                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = (int)patchResponse.StatusCode,
                        Mensagem = string.IsNullOrWhiteSpace(patchResponseBody)
                            ? "Falha ao atualizar GalaxPayId no servico Pedido."
                            : patchResponseBody
                    };
                }

                return new ResultDto
                {
                    Status = true,
                    StatusCode = (int)response.StatusCode,
                    Mensagem = string.Empty
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 499,
                    Mensagem = "Operacao cancelada."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao efetuar pagamento para PedidoId {PedidoId}.", request.PedidoId);
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Erro inesperado ao efetuar pagamento."
                };
            }
        }

        private static int? TryExtractGalaxPayIdAsInt(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;

                if (!root.TryGetProperty("galaxPayId", out var prop))
                    return null;

                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var n))
                    return n;

                var s = prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
                return int.TryParse(s, out var parsed) ? parsed : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
