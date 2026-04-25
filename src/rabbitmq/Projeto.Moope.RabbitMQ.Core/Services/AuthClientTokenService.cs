using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Auth;
using Projeto.Moope.RabbitMQ.Core.DTOs.Common;
using Projeto.Moope.RabbitMQ.Core.Helpers;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;

namespace Projeto.Moope.RabbitMQ.Core.Services
{
    public sealed class AuthClientTokenService : IAuthClientTokenService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly DownstreamApisOptions apis;
        private readonly ILogger<AuthClientTokenService> logger;

        public AuthClientTokenService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<AuthClientTokenService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.apis = apis.Value;
            this.logger = logger;
        }

        public async Task<ResultDto<ClientAccessTokenDto>> GetClientAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apis.Auth))
            {
                return new ResultDto<ClientAccessTokenDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:Auth nao configurado."
                };
            }

            if (string.IsNullOrWhiteSpace(apis.AuthClientId) || string.IsNullOrWhiteSpace(apis.AuthClientSecret))
            {
                return new ResultDto<ClientAccessTokenDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:AuthClientId/AuthClientSecret nao configurado."
                };
            }

            var url = UrlHelper.Combine(apis.Auth, "/api/auth/client/login");

            try
            {
                var httpClient = httpClientFactory.CreateClient();

                var basicRaw = $"{apis.AuthClientId}:{apis.AuthClientSecret}";
                var basicB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicRaw));

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicB64);

                using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Falha ao autenticar no Auth (client/login). Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseBody);

                    return new ResultDto<ClientAccessTokenDto>
                    {
                        Status = false,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = string.IsNullOrWhiteSpace(responseBody) ? "Falha ao autenticar no Auth." : responseBody
                    };
                }

                var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<ClientAccessTokenDto>>(responseBody, JsonOptions);
                if (envelope?.Data == null || string.IsNullOrWhiteSpace(envelope.Data.AccessToken))
                {
                    return new ResultDto<ClientAccessTokenDto>
                    {
                        Status = false,
                        StatusCode = 502,
                        Mensagem = "Resposta inesperada ao autenticar no Auth."
                    };
                }

                return new ResultDto<ClientAccessTokenDto>
                {
                    Status = true,
                    StatusCode = (int)response.StatusCode,
                    Mensagem = null,
                    Dados = envelope.Data
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new ResultDto<ClientAccessTokenDto>
                {
                    Status = false,
                    StatusCode = 499,
                    Mensagem = "Operacao cancelada."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao autenticar no Auth (client/login).");
                return new ResultDto<ClientAccessTokenDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Erro inesperado ao autenticar no Auth."
                };
            }
        }
    }
}

