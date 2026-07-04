using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.Cliente
{
    public sealed class ClienteListByVendedorService : IClienteListByVendedorService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public ClienteListByVendedorService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<IEnumerable<ClienteListItemDto>>> ExecutarAsync(string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente))
            {
                return new ResultDto<IEnumerable<ClienteListItemDto>>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Cliente) nao configurado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Cliente, "/api/cliente/vendedor");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<IEnumerable<ClienteListItemDto>>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao listar clientes do vendedor."
                };
            }

            var dados = await response.Content.ReadFromJsonAsync<List<ClienteListItemDto>>(JsonOptions, cancellationToken);
            if (dados == null)
            {
                return new ResultDto<IEnumerable<ClienteListItemDto>>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Cliente."
                };
            }

            return new ResultDto<IEnumerable<ClienteListItemDto>>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = dados
            };
        }
    }
}
