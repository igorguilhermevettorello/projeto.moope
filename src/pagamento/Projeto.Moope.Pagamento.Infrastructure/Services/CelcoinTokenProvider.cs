using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Pagamento.Core.Exceptions;
using Projeto.Moope.Pagamento.Infrastructure.Configurations;

namespace Projeto.Moope.Pagamento.Infrastructure.Services
{
    public class CelcoinTokenProvider : ICelcoinTokenProvider
    {
        private readonly ILogger<CelcoinTokenProvider> _logger;
        private readonly CelcoinPaymentsSettings _settings;
        private readonly HttpClient _httpClient;

        private readonly ConcurrentDictionary<string, CachedToken> _cache = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public CelcoinTokenProvider(
            ILogger<CelcoinTokenProvider> logger,
            IOptions<CelcoinPaymentsSettings> settings,
            HttpClient httpClient)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scope))
                throw new ArgumentException("Scope é obrigatório.", nameof(scope));

            if (_cache.TryGetValue(scope, out var existing) && !existing.IsExpired(_settings.TokenRefreshSkewSeconds))
                return existing.AccessToken;

            var sem = _locks.GetOrAdd(scope, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(scope, out existing) && !existing.IsExpired(_settings.TokenRefreshSkewSeconds))
                    return existing.AccessToken;

                var token = await AuthenticateAsync(scope, cancellationToken);
                var now = DateTimeOffset.UtcNow;
                var expiresAt = now.AddSeconds(token.ExpiresInSeconds > 0 ? token.ExpiresInSeconds : 600);

                _cache[scope] = new CachedToken(token.AccessToken, expiresAt);
                return token.AccessToken;
            }
            finally
            {
                sem.Release();
            }
        }

        public async Task<(string AccessToken, int ExpiresInSeconds, string TokenType, string Scope)> AuthenticateAsync(
            string scope,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.GalaxId) || string.IsNullOrWhiteSpace(_settings.GalaxHash))
                throw new InvalidOperationException("CelcoinPayments:GalaxId e CelcoinPayments:GalaxHash são obrigatórios.");

            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.GalaxId}:{_settings.GalaxHash}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, "token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

            if (!string.IsNullOrWhiteSpace(_settings.PartnerGalaxId) && !string.IsNullOrWhiteSpace(_settings.PartnerGalaxHash))
            {
                var partner = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.PartnerGalaxId}:{_settings.PartnerGalaxHash}"));
                request.Headers.TryAddWithoutValidation("AuthorizationPartner", partner);
            }

            // Assunção: OAuth2 client_credentials + scope via x-www-form-urlencoded (docs não exibem schema no fetch).
            var jsonBody = $@"{{
                ""grant_type"": ""authorization_code"",
                ""scope"": ""{scope}""
            }}";

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao autenticar no gateway Celcoin. Status: {StatusCode}. Body: {Body}",
                    (int)response.StatusCode, body);
                throw new CelcoinGatewayException("Falha ao autenticar no gateway Celcoin.", response.StatusCode, body);
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var tokenType = root.TryGetProperty("token_type", out var tt) ? tt.GetString() : "Bearer";
                var expiresIn = root.TryGetProperty("expires_in", out var ei) && ei.TryGetInt32(out var i) ? i : 600;
                var returnedScope = root.TryGetProperty("scope", out var sc) ? sc.GetString() : scope;

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new InvalidOperationException("Resposta de autenticação do gateway não contém access_token.");

                return (accessToken!, expiresIn, tokenType ?? "Bearer", returnedScope ?? scope);
            }
            catch (JsonException ex)
            {
                throw new CelcoinGatewayException("Resposta inválida ao autenticar no gateway Celcoin.", response.StatusCode, body, ex);
            }
        }

        private sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAtUtc)
        {
            public bool IsExpired(int refreshSkewSeconds)
            {
                var now = DateTimeOffset.UtcNow;
                return now >= ExpiresAtUtc.AddSeconds(-Math.Abs(refreshSkewSeconds));
            }
        }
    }
}

