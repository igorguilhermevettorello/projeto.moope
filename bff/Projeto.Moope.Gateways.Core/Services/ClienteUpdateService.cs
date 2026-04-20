using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class ClienteUpdateService : IClienteUpdateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IClienteGetByIdService _clienteGetByIdService;

        public ClienteUpdateService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IClienteGetByIdService clienteGetByIdService)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _clienteGetByIdService = clienteGetByIdService;
        }

        public async Task<ResultDto<ClienteDetailDto>> ExecutarAsync(ClienteUpdateDto request, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Cliente, Auth, Endereco) nao configurados."
                };
            }

            var existente = await _clienteGetByIdService.ExecutarAsync(request.Id, authorizationHeader, cancellationToken);
            if (!existente.Status || existente.Dados == null)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = existente.StatusCode,
                    Dados = null,
                    Mensagem = existente.Mensagem ?? "Cliente nao encontrado."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            var usuarioUrl = Utils.Combine(_apis.Auth, $"/api/usuario/{request.Id}");
            var usuarioBody = new
            {
                Id = request.Id,
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
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar usuario."
                };
            }

            var clienteUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/{request.Id}");
            var clienteBody = new
            {
                Id = request.Id,
                request.TipoPessoa,
                request.CpfCnpj,
                request.PercentualComissao,
                request.ChavePix,
                request.CodigoCupom,
                request.VendedorId
            };

            using var clienteRequest = new HttpRequestMessage(HttpMethod.Put, clienteUrl);
            Utils.AplicarAutorizacao(clienteRequest, authorizationHeader);
            clienteRequest.Content = JsonContent.Create(clienteBody, options: JsonOptions);

            using var clienteResponse = await httpClient.SendAsync(clienteRequest, cancellationToken);
            if (!clienteResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(clienteResponse, cancellationToken);
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar cliente."
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

            return await _clienteGetByIdService.ExecutarAsync(request.Id, authorizationHeader, cancellationToken);
        }

        private async Task<ResultDto<ClienteDetailDto>> ProcessarEnderecoAsync(
            HttpClient httpClient,
            Guid? enderecoIdAtual,
            Guid clienteId,
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
                    return new ResultDto<ClienteDetailDto>
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Dados = null,
                        Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar endereco."
                    };
                }

                return new ResultDto<ClienteDetailDto> { Status = true, StatusCode = StatusCodes.Status200OK };
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
                return new ResultDto<ClienteDetailDto>
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
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Endereco (Id ausente)."
                };
            }

            var vincularUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/{clienteId}/endereco/{novoEnderecoId}");
            using var vincularRequest = new HttpRequestMessage(HttpMethod.Patch, vincularUrl);
            Utils.AplicarAutorizacao(vincularRequest, authorizationHeader);

            using var vincularResponse = await httpClient.SendAsync(vincularRequest, cancellationToken);
            if (!vincularResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(vincularResponse, cancellationToken);
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao vincular endereco ao cliente."
                };
            }

            return new ResultDto<ClienteDetailDto> { Status = true, StatusCode = StatusCodes.Status200OK };
        }
    }
}

