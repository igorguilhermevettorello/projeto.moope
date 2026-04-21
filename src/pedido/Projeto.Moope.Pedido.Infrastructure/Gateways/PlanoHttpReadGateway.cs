using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Projeto.Moope.Pedido.Core.DTOs.Plano;
using Projeto.Moope.Pedido.Core.Interfaces.Gateways;

namespace Projeto.Moope.Pedido.Infrastructure.Gateways
{
    public class PlanoHttpReadGateway : IPlanoReadGateway
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<PlanoHttpReadGateway> _logger;

        public PlanoHttpReadGateway(HttpClient httpClient, ILogger<PlanoHttpReadGateway> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PlanoDetailDto?> ObterPorIdAsync(Guid planoId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/plano/{planoId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Falha ao consultar plano {PlanoId} na API de planos. Status {StatusCode}",
                    planoId,
                    (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PlanoDetailDto>(
                JsonOptions,
                cancellationToken);
        }
    }
}
