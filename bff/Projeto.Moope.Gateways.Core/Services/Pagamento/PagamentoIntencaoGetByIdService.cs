using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pagemento;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services.Pagamento
{
    public sealed class PagamentoIntencaoGetByIdService : IPagamentoIntencaoGetByIdService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public PagamentoIntencaoGetByIdService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto> ExecutarAsync(
            Guid idempotencyKey,
            string idempotencyKeyHeader,
            CancellationToken cancellationToken)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Idempotency-Key invalida."
                };
            }

            if (string.IsNullOrWhiteSpace(idempotencyKeyHeader))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Idempotency-Key e obrigatorio."
                };
            }

            if (string.IsNullOrWhiteSpace(_apis.Pagamento))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensagem = "DownstreamApis:Pagamento nao configurado."
                };
            }

            if (string.IsNullOrWhiteSpace(_apis.PagamentoIdempotencyKeyGeneratorApiKey))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensagem = "DownstreamApis:PagamentoIdempotencyKeyGeneratorApiKey nao configurado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Pagamento, $"/api/pagamentos/intencoes/{idempotencyKey}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKeyHeader);
            request.Headers.TryAddWithoutValidation("X-Api-Key", _apis.PagamentoIdempotencyKeyGeneratorApiKey.Trim());

            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Falha ao acessar o servico Pagamento."
                };
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                    return new ResultDto
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Mensagem = rs.Mensagem
                    };
                }

                return new ResultDto
                {
                    Status = true,
                    StatusCode = StatusCodes.Status200OK,
                    Mensagem = null
                };
            }
        }
    }
}

