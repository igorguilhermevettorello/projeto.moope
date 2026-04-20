using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class ClienteDeleteService : IClienteDeleteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IClienteGetByIdService _clienteGetByIdService;

        public ClienteDeleteService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IClienteGetByIdService clienteGetByIdService)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _clienteGetByIdService = clienteGetByIdService;
        }

        public async Task<ResultDto> ExecutarAsync(Guid clienteId, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (clienteId == Guid.Empty)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Id do cliente invalido."
                };
            }

            if (string.IsNullOrWhiteSpace(_apis.Cliente) || string.IsNullOrWhiteSpace(_apis.Auth))
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensagem = "DownstreamApis (Cliente, Auth) nao configurados."
                };
            }

            var existente = await _clienteGetByIdService.ExecutarAsync(clienteId, authorizationHeader, cancellationToken);
            if (!existente.Status)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = existente.StatusCode,
                    Mensagem = existente.Mensagem ?? "Cliente nao encontrado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            var clienteUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/{clienteId}");
            using var clienteRequest = new HttpRequestMessage(HttpMethod.Delete, clienteUrl);
            Utils.AplicarAutorizacao(clienteRequest, authorizationHeader);

            using var clienteResponse = await httpClient.SendAsync(clienteRequest, cancellationToken);
            if (!clienteResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(clienteResponse, cancellationToken);
                return new ResultDto
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao remover cliente."
                };
            }

            var usuarioUrl = Utils.Combine(_apis.Auth, $"/api/usuario/{clienteId}");
            using var usuarioRequest = new HttpRequestMessage(HttpMethod.Delete, usuarioUrl);
            Utils.AplicarAutorizacao(usuarioRequest, authorizationHeader);

            using var usuarioResponse = await httpClient.SendAsync(usuarioRequest, cancellationToken);
            if (!usuarioResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(usuarioResponse, cancellationToken);
                return new ResultDto
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao remover usuario."
                };
            }

            return new ResultDto
            {
                Status = true,
                StatusCode = StatusCodes.Status204NoContent
            };
        }
    }
}

