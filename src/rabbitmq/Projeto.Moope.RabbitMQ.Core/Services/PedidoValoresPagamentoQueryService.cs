using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Pedido;
using Projeto.Moope.RabbitMQ.Core.Helpers;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;

namespace Projeto.Moope.RabbitMQ.Core.Services
{
    public sealed class PedidoValoresPagamentoQueryService : IPedidoValoresPagamentoQueryService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly DownstreamApisOptions apis;
        private readonly ILogger<PedidoValoresPagamentoQueryService> logger;

        public PedidoValoresPagamentoQueryService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<PedidoValoresPagamentoQueryService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.apis = apis.Value;
            this.logger = logger;
        }

        public async Task<ResultDto<PedidoValoresPagamentoDto>> ObterValoresPagamentoAsync(
            Guid pedidoId,
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (pedidoId == Guid.Empty)
            {
                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = false,
                    StatusCode = 400,
                    Mensagem = "PedidoId invalido."
                };
            }

            if (string.IsNullOrWhiteSpace(apis.Pedido))
            {
                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:Pedido nao configurado."
                };
            }

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Authorization header nao informado."
                };
            }

            var url = UrlHelper.Combine(apis.Pedido, $"/api/pedido/{pedidoId}/valores-pagamento");

            try
            {
                var httpClient = httpClientFactory.CreateClient();

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

                using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Falha ao consultar valores de pagamento do Pedido. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseBody);

                    return new ResultDto<PedidoValoresPagamentoDto>
                    {
                        Status = false,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = string.IsNullOrWhiteSpace(responseBody) ? "Falha ao consultar servico de Pedido." : responseBody
                    };
                }

                var dto = JsonSerializer.Deserialize<PedidoValoresPagamentoDto>(responseBody, JsonOptions);
                if (dto == null || dto.PedidoId == Guid.Empty)
                {
                    return new ResultDto<PedidoValoresPagamentoDto>
                    {
                        Status = false,
                        StatusCode = 502,
                        Mensagem = "Resposta inesperada ao consultar valores do Pedido."
                    };
                }

                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = true,
                    StatusCode = (int)response.StatusCode,
                    Mensagem = null,
                    Dados = dto
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = false,
                    StatusCode = 499,
                    Mensagem = "Operacao cancelada."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao consultar valores do PedidoId {PedidoId}.", pedidoId);
                return new ResultDto<PedidoValoresPagamentoDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Erro inesperado ao consultar valores do Pedido."
                };
            }
        }
    }
}

