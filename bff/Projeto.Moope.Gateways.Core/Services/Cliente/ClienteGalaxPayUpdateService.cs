using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services.Cliente
{
    public class ClienteGalaxPayUpdateService : IClienteGalaxPayUpdateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IClienteGetByIdService _clienteGetByIdService;

        public ClienteGalaxPayUpdateService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IClienteGetByIdService clienteGetByIdService)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _clienteGetByIdService = clienteGetByIdService;
        }

        public async Task<ResultDto<ClienteDetailDto>> ExecutarAsync(
            ClienteGalaxPayUpdateDto request,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "Request invalido."
                };
            }

            if (request.ClienteId == Guid.Empty)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "ClienteId invalido."
                };
            }

            if (request.GalaxPayCustomerId <= 0)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "GalaxPayCustomerId invalido."
                };
            }

            if (string.IsNullOrWhiteSpace(_apis.Cliente))
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Cliente) nao configurado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Cliente, $"/api/cliente/{request.ClienteId}/galaxpay/{request.GalaxPayCustomerId}");

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, url);
            Utils.AplicarAutorizacao(patchRequest, authorizationHeader);

            using var response = await httpClient.SendAsync(patchRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar GalaxPayId do cliente."
                };
            }

            return await _clienteGetByIdService.ExecutarAsync(request.ClienteId, authorizationHeader, cancellationToken);
        }
    }
}
