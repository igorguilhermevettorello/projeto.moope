using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services.Vendedor
{
    public class VendedorGetByCupom : IVendedorGetByCupom
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public VendedorGetByCupom(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<VendedorDetailDto>> ExecutarAsync(string cupom, string? authorizationHeader, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var cupomUrl = Utils.Combine(_apis.Vendedor!, $"/api/vendedor/cupom/{Uri.EscapeDataString(cupom.Trim())}");
            using var cupomRequest = new HttpRequestMessage(HttpMethod.Get, cupomUrl);
            Utils.AplicarAutorizacao(cupomRequest, authorizationHeader);
            using var cupomResponse = await httpClient.SendAsync(cupomRequest, cancellationToken);

            if (cupomResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    Mensagem = "Cupom de vendedor invalido ou nao encontrado.",
                    Dados = null,
                };
            }

            if (!cupomResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(cupomResponse, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    Mensagem = rs.Mensagem,
                    Dados = null,
                };
            }

            var cupomId = await Utils.LerGuidRespostaAsync(cupomResponse, cancellationToken);
            if (cupomId == null)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Resposta invalida do servico Vendedor (Id ausente).",
                    Dados = null,
                };
            }

            return new ResultDto<VendedorDetailDto>
            {
                Status = true,
                Dados = new VendedorDetailDto
                { 
                    Id = Guid.Parse(cupomId.ToString())
                },
            };
        }
    }
}
