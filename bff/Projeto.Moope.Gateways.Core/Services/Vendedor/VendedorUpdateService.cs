using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.Vendedor
{
    public class VendedorUpdateService : IVendedorUpdateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IVendedorGetByIdService _vendedorGetByIdService;

        public VendedorUpdateService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IVendedorGetByIdService vendedorGetByIdService)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _vendedorGetByIdService = vendedorGetByIdService;
        }

        public async Task<ResultDto<VendedorDetailDto>> ExecutarAsync(VendedorUpdateDto request, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Vendedor)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Vendedor, Auth, Endereco) nao configurados."
                };
            }

            var existente = await _vendedorGetByIdService.ExecutarAsync(request.Id, authorizationHeader, cancellationToken);
            if (!existente.Status || existente.Dados == null)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = existente.StatusCode,
                    Dados = null,
                    Mensagem = existente.Mensagem ?? "Vendedor nao encontrado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            var usuarioUrl = Utils.Combine(_apis.Auth, $"/api/usuario/{request.Id}");
            var usuarioBody = new
            {
                request.Id,
                request.Nome,
                request.Email,
                request.CpfCnpj,
                request.Telefone,
                request.TipoPessoa,
                NomeFantasia = request.NomeFantasia ?? string.Empty,
                InscricaoEstadual = request.InscricaoEstadual ?? string.Empty,
                request.VendedorId
            };

            using var usuarioRequest = new HttpRequestMessage(HttpMethod.Put, usuarioUrl);
            Utils.AplicarAutorizacao(usuarioRequest, authorizationHeader);
            usuarioRequest.Content = JsonContent.Create(usuarioBody, options: JsonOptions);

            using var usuarioResponse = await httpClient.SendAsync(usuarioRequest, cancellationToken);
            if (!usuarioResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(usuarioResponse, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar usuario."
                };
            }

            var vendedorUrl = Utils.Combine(_apis.Vendedor, $"/api/vendedor/{request.Id}");
            var vendedorBody = new
            {
                request.Id,
                request.TipoPessoa,
                request.CpfCnpj,
                request.PercentualComissao,
                request.ChavePix,
                request.CodigoCupom,
                request.VendedorId
            };

            using var vendedorRequest = new HttpRequestMessage(HttpMethod.Put, vendedorUrl);
            Utils.AplicarAutorizacao(vendedorRequest, authorizationHeader);
            vendedorRequest.Content = JsonContent.Create(vendedorBody, options: JsonOptions);

            using var vendedorResponse = await httpClient.SendAsync(vendedorRequest, cancellationToken);
            if (!vendedorResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(vendedorResponse, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar vendedor."
                };
            }

            if (request.Endereco != null)
            {
                var enderecoResultado = await ProcessarEnderecoAsync(
                    httpClient,
                    existente.Dados.EnderecoId,
                    request.Id,
                    request.Endereco,
                    authorizationHeader,
                    cancellationToken);

                if (!enderecoResultado.Status)
                    return enderecoResultado;
            }

            return await _vendedorGetByIdService.ExecutarAsync(request.Id, authorizationHeader, cancellationToken);
        }

        private async Task<ResultDto<VendedorDetailDto>> ProcessarEnderecoAsync(
            HttpClient httpClient,
            Guid? enderecoIdAtual,
            Guid vendedorId,
            EnderecoUpdateDto endereco,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (enderecoIdAtual.HasValue && enderecoIdAtual.Value != Guid.Empty)
            {
                var enderecoUrl = Utils.Combine(_apis.Endereco, $"/api/endereco/{enderecoIdAtual.Value}");
                var enderecoBody = new
                {
                    Id = enderecoIdAtual.Value,
                    endereco.Cep,
                    endereco.Logradouro,
                    Numero = endereco.Numero ?? string.Empty,
                    Complemento = endereco.Complemento ?? string.Empty,
                    endereco.Bairro,
                    endereco.Cidade,
                    endereco.Estado
                };

                using var enderecoRequest = new HttpRequestMessage(HttpMethod.Put, enderecoUrl);
                Utils.AplicarAutorizacao(enderecoRequest, authorizationHeader);
                enderecoRequest.Content = JsonContent.Create(enderecoBody, options: JsonOptions);

                using var enderecoResponse = await httpClient.SendAsync(enderecoRequest, cancellationToken);
                if (!enderecoResponse.IsSuccessStatusCode)
                {
                    var rs = await Utils.FalhaDownstreamAsync(enderecoResponse, cancellationToken);
                    return new ResultDto<VendedorDetailDto>
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Dados = null,
                        Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar endereco."
                    };
                }

                return new ResultDto<VendedorDetailDto> { Status = true, StatusCode = StatusCodes.Status200OK };
            }

            var criarUrl = Utils.Combine(_apis.Endereco, "/api/endereco");
            var criarBody = new
            {
                endereco.Cep,
                endereco.Logradouro,
                Numero = endereco.Numero ?? string.Empty,
                Complemento = endereco.Complemento ?? string.Empty,
                endereco.Bairro,
                endereco.Cidade,
                endereco.Estado
            };

            using var criarRequest = new HttpRequestMessage(HttpMethod.Post, criarUrl);
            Utils.AplicarAutorizacao(criarRequest, authorizationHeader);
            criarRequest.Content = JsonContent.Create(criarBody, options: JsonOptions);

            using var criarResponse = await httpClient.SendAsync(criarRequest, cancellationToken);
            if (!criarResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(criarResponse, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar endereco."
                };
            }

            var novoEnderecoId = await Utils.LerGuidRespostaAsync(criarResponse, cancellationToken);
            if (novoEnderecoId == null)
            {
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Endereco (Id ausente)."
                };
            }

            var vincularUrl = Utils.Combine(_apis.Vendedor, $"/api/vendedor/{vendedorId}/endereco/{novoEnderecoId}");
            using var vincularRequest = new HttpRequestMessage(HttpMethod.Patch, vincularUrl);
            Utils.AplicarAutorizacao(vincularRequest, authorizationHeader);

            using var vincularResponse = await httpClient.SendAsync(vincularRequest, cancellationToken);
            if (!vincularResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(vincularResponse, cancellationToken);
                return new ResultDto<VendedorDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao vincular endereco ao vendedor."
                };
            }

            return new ResultDto<VendedorDetailDto> { Status = true, StatusCode = StatusCodes.Status200OK };
        }
    }
}
