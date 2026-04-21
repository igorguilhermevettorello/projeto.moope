using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class AuthClientLoginService : IAuthClientLoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public AuthClientLoginService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }           
        
        public async Task<Result<string>> ExecutarAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.AuthClientId)
                || string.IsNullOrWhiteSpace(_apis.AuthClientSecret))
            {
                return Utils.FalhaConfig<string>("DownstreamApis (Auth, AuthClientId, AuthClientSecret) sao obrigatorios para obter token via client/login.");
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Auth, "/api/auth/client/login");

            var raw = $"{_apis.AuthClientId}:{_apis.AuthClientSecret}";
            var basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {basic}");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var node = JsonNode.Parse(json);
                var accessToken = node?["data"]?["accessToken"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(accessToken))
                    return Utils.FalhaConfig<string>("Nao foi possivel obter token do servico Auth (client/login).");

                return new Result<string>
                {
                    Status = true,
                    StatusCode = 200,
                    Dados = $"Bearer {accessToken}"
                };
            }
            catch (JsonException)
            {
                return Utils.FalhaConfig<string>("Resposta invalida do servico Auth (client/login).");
            }
        }
    }
}
