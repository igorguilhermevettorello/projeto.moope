using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class VendedorGetByIdService : IVendedorGetByIdService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public VendedorGetByIdService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<VendedorDetailDto>> ExecutarAsync(Guid vendedorId, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (vendedorId == Guid.Empty)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "Id do vendedor invalido."
                };
            }

            if (string.IsNullOrWhiteSpace(_apis.Vendedor))
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Vendedor) nao configurado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Vendedor, $"/api/vendedor/{vendedorId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Dados = null,
                    Mensagem = "Vendedor nao encontrado."
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao obter vendedor."
                };
            }

            var dados = await response.Content.ReadFromJsonAsync<VendedorDetailDto>(JsonOptions, cancellationToken);
            if (dados == null)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Vendedor."
                };
            }

            if (dados.EnderecoId is Guid enderecoId && enderecoId != Guid.Empty)
            {
                if (string.IsNullOrWhiteSpace(_apis.Endereco))
                {
                    return new ResultDto<VendedorDetailDto>
                    {
                        Status = false,
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Dados = null,
                        Mensagem = "DownstreamApis (Endereco) nao configurado."
                    };
                }

                var enderecoUrl = Utils.Combine(_apis.Endereco, $"/api/endereco/{enderecoId}");
                using var enderecoRequest = new HttpRequestMessage(HttpMethod.Get, enderecoUrl);
                Utils.AplicarAutorizacao(enderecoRequest, authorizationHeader);

                using var enderecoResponse = await httpClient.SendAsync(enderecoRequest, cancellationToken);
                if (!enderecoResponse.IsSuccessStatusCode)
                {
                    var rs = await Utils.FalhaDownstreamAsync(enderecoResponse, cancellationToken);
                    return new ResultDto<VendedorDetailDto>
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Dados = null,
                        Mensagem = rs.Mensagem ?? "Erro desconhecido ao obter endereco."
                    };
                }

                var endereco = await enderecoResponse.Content.ReadFromJsonAsync<EnderecoDetailDto>(JsonOptions, cancellationToken);
                if (endereco == null)
                {
                    return new ResultDto<VendedorDetailDto>
                    {
                        Status = false,
                        StatusCode = StatusCodes.Status502BadGateway,
                        Dados = null,
                        Mensagem = "Resposta invalida do servico Endereco."
                    };
                }

                AplicarEnderecoNaResposta(dados, endereco);
            }
            else
            {
                dados.Endereco = null;
            }

            return new ResultDto<VendedorDetailDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = dados
            };
        }

        private static void AplicarEnderecoNaResposta(VendedorDetailDto dados, EnderecoDetailDto endereco)
        {
            dados.EnderecoId = endereco.Id;
            dados.Endereco = endereco;
        }
    }
}
