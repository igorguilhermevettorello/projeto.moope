using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class ClienteGetByIdService : IClienteGetByIdService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public ClienteGetByIdService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<ClienteDetailDto>> ExecutarAsync(Guid clienteId, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (clienteId == Guid.Empty)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "Id do cliente invalido."
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
            var url = Utils.Combine(_apis.Cliente, $"/api/cliente/{clienteId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Dados = null,
                    Mensagem = "Cliente nao encontrado."
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao obter cliente."
                };
            }

            var downstream = await response.Content.ReadFromJsonAsync<ClienteDownstreamDetail>(JsonOptions, cancellationToken);
            if (downstream == null)
            {
                return new ResultDto<ClienteDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Cliente."
                };
            }

            var dados = MapearDownstream(downstream);

            if (dados.EnderecoId is Guid enderecoId && enderecoId != Guid.Empty)
            {
                if (string.IsNullOrWhiteSpace(_apis.Endereco))
                {
                    return new ResultDto<ClienteDetailDto>
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
                    return new ResultDto<ClienteDetailDto>
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
                    return new ResultDto<ClienteDetailDto>
                    {
                        Status = false,
                        StatusCode = StatusCodes.Status502BadGateway,
                        Dados = null,
                        Mensagem = "Resposta invalida do servico Endereco."
                    };
                }

                dados.Endereco = endereco;
            }
            else if (PossuiEnderecoInline(downstream))
            {
                dados.Endereco = new EnderecoDetailDto
                {
                    Cep = downstream.Cep,
                    Logradouro = downstream.Logradouro,
                    Numero = downstream.Numero,
                    Complemento = downstream.Complemento,
                    Bairro = downstream.Bairro,
                    Cidade = downstream.Cidade,
                    Estado = downstream.Estado
                };
            }
            else
            {
                dados.Endereco = null;
            }

            return new ResultDto<ClienteDetailDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = dados
            };
        }

        private static ClienteDetailDto MapearDownstream(ClienteDownstreamDetail downstream)
        {
            return new ClienteDetailDto
            {
                Id = downstream.Id,
                Nome = downstream.Nome,
                Email = downstream.Email,
                TipoPessoa = downstream.TipoPessoa,
                CpfCnpj = downstream.CpfCnpj,
                Telefone = downstream.Telefone,
                Ativo = downstream.Ativo,
                ChavePix = downstream.ChavePix,
                PercentualComissao = downstream.PercentualComissao,
                NomeFantasia = downstream.NomeFantasia,
                InscricaoEstadual = downstream.InscricaoEstadual,
                CodigoCupom = downstream.CodigoCupom,
                EnderecoId = downstream.EnderecoId
            };
        }

        private static bool PossuiEnderecoInline(ClienteDownstreamDetail downstream)
        {
            return !string.IsNullOrWhiteSpace(downstream.Cep)
                || !string.IsNullOrWhiteSpace(downstream.Logradouro)
                || !string.IsNullOrWhiteSpace(downstream.Cidade)
                || !string.IsNullOrWhiteSpace(downstream.Estado);
        }

        private sealed class ClienteDownstreamDetail
        {
            public Guid Id { get; set; }
            public string? Nome { get; set; }
            public string? Email { get; set; }
            public string? TipoPessoa { get; set; }
            public string? CpfCnpj { get; set; }
            public string? Telefone { get; set; }
            public bool Ativo { get; set; }
            public string? ChavePix { get; set; }
            public decimal? PercentualComissao { get; set; }
            public string? NomeFantasia { get; set; }
            public string? InscricaoEstadual { get; set; }
            public string? CodigoCupom { get; set; }
            public Guid? EnderecoId { get; set; }

            public string? Cep { get; set; }
            public string? Logradouro { get; set; }
            public string? Numero { get; set; }
            public string? Complemento { get; set; }
            public string? Bairro { get; set; }
            public string? Cidade { get; set; }
            public string? Estado { get; set; }
        }
    }
}

