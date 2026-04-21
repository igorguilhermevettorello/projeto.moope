using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Pagamento.Core.Exceptions;
using Projeto.Moope.Pagamento.Core.Interfaces.Gateways;
using Projeto.Moope.Pagamento.Core.Services.Models;
using Projeto.Moope.Pagamento.Infrastructure.Configurations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Projeto.Moope.Pagamento.Infrastructure.Services
{
    public class CelcoinPaymentGatewayClient : ICelcoinPaymentGatewayClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly ILogger<CelcoinPaymentGatewayClient> _logger;
        private readonly CelcoinPaymentsSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ICelcoinTokenProvider _tokenProvider;

        public CelcoinPaymentGatewayClient(
            ILogger<CelcoinPaymentGatewayClient> logger,
            IOptions<CelcoinPaymentsSettings> settings,
            HttpClient httpClient,
            ICelcoinTokenProvider tokenProvider)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;

            _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _settings.TimeoutSeconds));
        }

        public async Task<JsonElement> AutenticarAsync(string scope, CancellationToken cancellationToken = default)
        {
            var token = await _tokenProvider.AuthenticateAsync(scope, cancellationToken);
            var obj = new
            {
                accessToken = token.AccessToken,
                tokenType = token.TokenType,
                expiresIn = token.ExpiresInSeconds,
                scope = token.Scope
            };

            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(obj, JsonOptions));
            return doc.RootElement.Clone();
        }

        public Task<JsonElement> CriarClienteAsync(CriarClienteGatewayRequestDto request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, "customers", scope: "customers.write", request.Payload, cancellationToken);

        public Task<JsonElement> ListarClientesAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Get, BuildPath("customers", query), scope: "customers.read", body: null, cancellationToken);

        public Task<JsonElement> CriarPlanoAsync(object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, "plans", scope: "plans.write", request, cancellationToken);

        public Task<JsonElement> ListarPlanosAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Get, BuildPath("plans", query), scope: "plans.read", body: null, cancellationToken);

        public Task<JsonElement> CriarCartaoAsync(string customerId, string typeId, object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, $"cards/{customerId}/{typeId}", scope: "cards.write", request, cancellationToken);

        public Task<JsonElement> CriarAssinaturaAsync(object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, "subscriptions", scope: "subscriptions.write", request, cancellationToken);

        public Task<JsonElement> CriarAssinaturaManualAsync(object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, "subscriptions/manual", scope: "subscriptions.write", request, cancellationToken);

        public Task<JsonElement> AdicionarTransacaoEmAssinaturaAsync(string subscriptionId, string typeId, object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, $"transactions/{subscriptionId}/{typeId}/add", scope: "subscriptions.write", request, cancellationToken);

        public Task<JsonElement> ListarTransacoesAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Get, BuildPath("transactions", query), scope: "transactions.read", body: null, cancellationToken);

        public async Task<JsonElement> CancelarTransacaoAsync(string transactionId, string typeId, CancellationToken cancellationToken = default)
        {
            return await SendJsonAsync(HttpMethod.Delete, $"transactions/{transactionId}/{typeId}", scope: "transactions.write", body: null, cancellationToken);
        }

        public Task<JsonElement> CriarCobrancaAvulsaAsync(object request, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Post, "charges", scope: "charges.write", request, cancellationToken);

        public Task<JsonElement> ListarCobrancasAvulsasAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default) =>
            SendJsonAsync(HttpMethod.Get, BuildPath("charges", query), scope: "charges.read", body: null, cancellationToken);

        private async Task<JsonElement> SendJsonAsync(
            HttpMethod method,
            string path,
            string scope,
            object? body,
            CancellationToken cancellationToken)
        {
            var accessToken = await _tokenProvider.GetAccessTokenAsync(scope, cancellationToken);

            using var request = new HttpRequestMessage(method, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gateway Celcoin retornou erro. {Method} {Path} -> {StatusCode}. Body: {Body}",
                    method.Method, path, (int)response.StatusCode, responseBody);
                throw new CelcoinGatewayException("Gateway Celcoin retornou erro.", response.StatusCode, responseBody);
            }

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                using var emptyDoc = JsonDocument.Parse("null");
                return emptyDoc.RootElement.Clone();
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                return doc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                throw new CelcoinGatewayException("Resposta do gateway não é um JSON válido.", response.StatusCode, responseBody, ex);
            }
        }

        private static string BuildPath(string path, IDictionary<string, string?>? query)
        {
            if (query == null || query.Count == 0)
                return path;

            var pairs = query
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
                .ToArray();

            if (pairs.Length == 0)
                return path;

            return $"{path}?{string.Join("&", pairs)}";
        }
    }
}

