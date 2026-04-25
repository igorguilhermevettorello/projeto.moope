using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.Helpers;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;

namespace Projeto.Moope.RabbitMQ.Core.Services
{
    public sealed class ClienteProvisioningService : IClienteProvisioningService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly DownstreamApisOptions apis;
        private readonly ILogger<ClienteProvisioningService> logger;

        public ClienteProvisioningService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<ClienteProvisioningService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.apis = apis.Value;
            this.logger = logger;
        }

        public async Task<ResultDto> CriarClienteSeNaoExistirAsync(
            Guid usuarioId,
            string email,
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apis.Cliente))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "DownstreamApis:Cliente nao configurado."
                };
            }

            if (usuarioId == Guid.Empty)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 400,
                    Mensagem = "usuarioId inválido."
                };
            }

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 400,
                    Mensagem = "AuthorizationHeader é obrigatório."
                };
            }

            var url = UrlHelper.Combine(apis.Cliente, "/api/cliente");

            try
            {
                var httpClient = httpClientFactory.CreateClient();

                var body = new { usuarioId };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body, options: JsonOptions)
                };
                httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

                using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return new ResultDto
                    {
                        Status = true,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = null
                    };
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    logger.LogInformation("Cliente já existe no serviço Cliente. UsuarioId: {UsuarioId}. Email: {Email}", usuarioId, email);
                    return new ResultDto
                    {
                        Status = true,
                        StatusCode = (int)response.StatusCode,
                        Mensagem = "Cliente já existe."
                    };
                }

                logger.LogWarning(
                    "Falha ao criar cliente no serviço Cliente. UsuarioId: {UsuarioId}. Status: {StatusCode}. Body: {Body}",
                    usuarioId,
                    (int)response.StatusCode,
                    responseBody);

                return new ResultDto
                {
                    Status = false,
                    StatusCode = (int)response.StatusCode,
                    Mensagem = string.IsNullOrWhiteSpace(responseBody) ? "Falha ao criar cliente." : responseBody
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
                logger.LogError(ex, "Erro inesperado ao criar cliente. UsuarioId: {UsuarioId}. Email: {Email}", usuarioId, email);
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 500,
                    Mensagem = "Erro inesperado ao criar cliente."
                };
            }
        }
    }
}

