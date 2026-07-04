using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido;
using Projeto.Moope.Gateways.Core.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.Pedido
{
    public sealed class PedidoListService : IPedidoListService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly ILogger<PedidoListService> _logger;

        public PedidoListService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            ILogger<PedidoListService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _logger = logger;
        }

        public async Task<ResultDto<IEnumerable<PedidoListItemDto>>> ExecutarAsync(
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Pedido)
                || string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Vendedor))
            {
                return new ResultDto<IEnumerable<PedidoListItemDto>>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Pedido, Cliente, Vendedor) nao configurados."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var pedidoUrl = Utils.Combine(_apis.Pedido, "/api/pedido");

            using var pedidoRequest = new HttpRequestMessage(HttpMethod.Get, pedidoUrl);
            Utils.AplicarAutorizacao(pedidoRequest, authorizationHeader);

            using var pedidoResponse = await httpClient.SendAsync(pedidoRequest, cancellationToken);
            if (!pedidoResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(pedidoResponse, cancellationToken);
                return new ResultDto<IEnumerable<PedidoListItemDto>>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao listar pedidos."
                };
            }

            var pedidosBase = await pedidoResponse.Content.ReadFromJsonAsync<List<PedidoListBaseDto>>(JsonOptions, cancellationToken);
            if (pedidosBase == null)
            {
                return new ResultDto<IEnumerable<PedidoListItemDto>>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Pedido."
                };
            }

            var clienteIds = pedidosBase
                .Select(p => p.ClienteId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var vendedorIds = pedidosBase
                .Where(p => p.VendedorId.HasValue && p.VendedorId.Value != Guid.Empty)
                .Select(p => p.VendedorId!.Value)
                .Distinct()
                .ToList();

            var clientesPorId = await ObterClientesPorIdAsync(httpClient, clienteIds, authorizationHeader, cancellationToken);
            var vendedoresPorId = await ObterVendedoresPorIdAsync(httpClient, vendedorIds, authorizationHeader, cancellationToken);

            var itens = pedidosBase.Select(p =>
            {
                clientesPorId.TryGetValue(p.ClienteId, out var cliente);
                vendedoresPorId.TryGetValue(p.VendedorId ?? Guid.Empty, out var vendedor);

                return new PedidoListItemDto
                {
                    Id = p.Id,
                    Plano = p.Plano,
                    Valor = p.Valor,
                    Quantidade = p.Quantidade,
                    Total = p.Total,
                    ClienteNome = cliente?.Nome ?? string.Empty,
                    ClienteCidade = cliente?.Cidade,
                    ClienteEstado = cliente?.Estado,
                    VendedorNome = !p.VendedorId.HasValue ? null : vendedor?.Nome ?? string.Empty
                };
            }).ToList();

            return new ResultDto<IEnumerable<PedidoListItemDto>>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = itens
            };
        }

        private async Task<Dictionary<Guid, ClienteResumo>> ObterClientesPorIdAsync(
            HttpClient httpClient,
            IReadOnlyList<Guid> clienteIds,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (clienteIds.Count == 0)
                return new Dictionary<Guid, ClienteResumo>();

            var resultados = await Task.WhenAll(
                clienteIds.Select(id => ObterClientePorIdAsync(httpClient, id, authorizationHeader, cancellationToken)));

            return resultados
                .Where(c => c != null)
                .Select(c => c!)
                .ToDictionary(c => c.Id);
        }

        private async Task<Dictionary<Guid, VendedorResumo>> ObterVendedoresPorIdAsync(
            HttpClient httpClient,
            IReadOnlyList<Guid> vendedorIds,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (vendedorIds.Count == 0)
                return new Dictionary<Guid, VendedorResumo>();

            var resultados = await Task.WhenAll(
                vendedorIds.Select(id => ObterVendedorPorIdAsync(httpClient, id, authorizationHeader, cancellationToken)));

            return resultados
                .Where(v => v != null)
                .Select(v => v!)
                .ToDictionary(v => v.Id);
        }

        private async Task<ClienteResumo?> ObterClientePorIdAsync(
            HttpClient httpClient,
            Guid clienteId,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            var url = Utils.Combine(_apis.Cliente, $"/api/cliente/{clienteId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Falha ao consultar cliente {ClienteId} na API de clientes. Status {StatusCode}",
                    clienteId,
                    (int)response.StatusCode);
                return null;
            }

            var downstream = await response.Content.ReadFromJsonAsync<ClienteResumo>(JsonOptions, cancellationToken);
            return downstream;
        }

        private async Task<VendedorResumo?> ObterVendedorPorIdAsync(
            HttpClient httpClient,
            Guid vendedorId,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            var url = Utils.Combine(_apis.Vendedor, $"/api/vendedor/{vendedorId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Falha ao consultar vendedor {VendedorId} na API de vendedores. Status {StatusCode}",
                    vendedorId,
                    (int)response.StatusCode);
                return null;
            }

            var downstream = await response.Content.ReadFromJsonAsync<VendedorResumo>(JsonOptions, cancellationToken);
            return downstream;
        }

        private sealed class ClienteResumo
        {
            public Guid Id { get; set; }
            public string? Nome { get; set; }
            public string? Cidade { get; set; }
            public string? Estado { get; set; }
        }

        private sealed class VendedorResumo
        {
            public Guid Id { get; set; }
            public string? Nome { get; set; }
        }
    }
}
