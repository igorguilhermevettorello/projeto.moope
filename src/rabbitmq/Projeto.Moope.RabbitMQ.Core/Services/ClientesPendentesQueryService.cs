using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Usuario;
using Projeto.Moope.RabbitMQ.Core.Helpers;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;

namespace Projeto.Moope.RabbitMQ.Core.Services
{
    public sealed class ClientesPendentesQueryService : IClientesPendentesQueryService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly DownstreamApisOptions apis;
        private readonly ILogger<ClientesPendentesQueryService> logger;

        public ClientesPendentesQueryService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<ClientesPendentesQueryService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.apis = apis.Value;
            this.logger = logger;
        }

        public async Task<ResultDto<IReadOnlyList<ClientePendenteDto>>> ListarClientesPendentesAsync(
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apis.Auth))
            {
                return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:Auth nao configurado."
                };
            }

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                {
                    Status = false,
                    StatusCode = 400,
                    Mensagem = "AuthorizationHeader é obrigatório."
                };
            }

            var url = UrlHelper.Combine(apis.Auth, "/api/usuario/clientes-pendentes");

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
                        "Falha ao listar clientes pendentes. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseBody);

                    return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                    {
                        Status = false,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = string.IsNullOrWhiteSpace(responseBody) ? "Falha ao listar clientes pendentes." : responseBody
                    };
                }

                var data = JsonSerializer.Deserialize<List<ClientePendenteDto>>(responseBody, JsonOptions) ?? new List<ClientePendenteDto>();
                return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                {
                    Status = true,
                    StatusCode = (int)response.StatusCode,
                    Mensagem = null,
                    Dados = data
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                {
                    Status = false,
                    StatusCode = 499,
                    Mensagem = "Operacao cancelada."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao listar clientes pendentes.");
                return new ResultDto<IReadOnlyList<ClientePendenteDto>>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Erro inesperado ao listar clientes pendentes."
                };
            }
        }
    }
}

