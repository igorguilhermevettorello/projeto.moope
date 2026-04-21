using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.Vendedor
{
    public class VendedorCreateService : IVendedorCreateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public VendedorCreateService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<VendedorCreateResultDto>> ExecutarAsync(VendedorCreateDto request, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Vendedor)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Vendedor, Auth, Endereco) nao configurados."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            var authUrl = Utils.Combine(_apis.Auth, "/api/usuario");
            var usuarioBody = new
            {
                request.Nome,
                request.Email,
                request.CpfCnpj,
                request.Telefone,
                request.TipoPessoa,
                TipoUsuario = TipoUsuario.Vendedor,
                request.Senha,
                request.Confirmacao,
                NomeFantasia = request.NomeFantasia ?? string.Empty,
                InscricaoEstadual = request.InscricaoEstadual ?? string.Empty
            };

            using var usuarioRequest = new HttpRequestMessage(HttpMethod.Post, authUrl);
            Utils.AplicarAutorizacao(usuarioRequest, authorizationHeader);
            usuarioRequest.Content = JsonContent.Create(usuarioBody, options: JsonOptions);

            using var usuarioResponse = await httpClient.SendAsync(usuarioRequest, cancellationToken);
            if (!usuarioResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(usuarioResponse, cancellationToken);
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar usuario."
                };
            }
                

            var usuarioId = await Utils.LerGuidRespostaAsync(usuarioResponse, cancellationToken);
            if (usuarioId == null)
            {
                //return new CadastroRepresentanteOrchestrationResult
                //{
                //    Sucesso = false,
                //    StatusCode = StatusCodes.Status502BadGateway,
                //    CorpoErro = new { mensagem = "Resposta invalida do servico Auth (Id ausente)." }
                //};

                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Auth (Id ausente)."
                };
            }

            var vendedorUrl = Utils.Combine(_apis.Vendedor, "/api/vendedor");
            var vendedorBody = new
            {
                request.TipoPessoa,
                request.CpfCnpj,
                request.PercentualComissao,
                request.ChavePix,
                request.CodigoCupom,
                UsuarioId = (Guid)usuarioId
            };

            using var vendedorRequest = new HttpRequestMessage(HttpMethod.Post, vendedorUrl);
            Utils.AplicarAutorizacao(vendedorRequest, authorizationHeader);
            vendedorRequest.Content = JsonContent.Create(vendedorBody, options: JsonOptions);
            using var vendedorResponse = await httpClient.SendAsync(vendedorRequest, cancellationToken);

            if (!vendedorResponse.IsSuccessStatusCode) 
            {
                var rs = await Utils.FalhaDownstreamAsync(vendedorResponse, cancellationToken);
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar vendedor."
                };
            }
                

            var vendedorId = await Utils.LerGuidRespostaAsync(vendedorResponse, cancellationToken);
            if (vendedorId == null)
            {
                //return new CadastroRepresentanteOrchestrationResult
                //{
                //    Sucesso = false,
                //    StatusCode = StatusCodes.Status502BadGateway,
                //    CorpoErro = new { mensagem = "Resposta invalida do servico Vendedor (Id ausente)." }
                //};

                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Vendedor (Id ausente)."
                };
            }



            var enderecoUrl = Utils.Combine(_apis.Endereco, "/api/endereco");
            var enderecoBody = new
            {
                request.Endereco.Cep,
                request.Endereco.Logradouro,
                Numero = request.Endereco.Numero ?? string.Empty,
                Complemento = request.Endereco.Complemento ?? string.Empty,
                request.Endereco.Bairro,
                request.Endereco.Cidade,
                request.Endereco.Estado
            };

            using var enderecoRequest = new HttpRequestMessage(HttpMethod.Post, enderecoUrl);
            Utils.AplicarAutorizacao(enderecoRequest, authorizationHeader);
            enderecoRequest.Content = JsonContent.Create(enderecoBody, options: JsonOptions);

            using var enderecoResponse = await httpClient.SendAsync(enderecoRequest, cancellationToken);
            if (!enderecoResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(enderecoResponse, cancellationToken);
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar endereco."
                };
            }
                

            var enderecoId = await Utils.LerGuidRespostaAsync(enderecoResponse, cancellationToken);
            if (enderecoId == null)
            {
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Endereco (Id ausente)."
                };
            }

            var atualizarEnderecoUrl = Utils.Combine(_apis.Vendedor, $"/api/vendedor/{usuarioId}/endereco/{enderecoId}");
            using var atualizarEnderecoRequest = new HttpRequestMessage(HttpMethod.Patch, atualizarEnderecoUrl);
            Utils.AplicarAutorizacao(atualizarEnderecoRequest, authorizationHeader);

            using var atualizarEnderecoResponse = await httpClient.SendAsync(atualizarEnderecoRequest, cancellationToken);
            if (!atualizarEnderecoResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(atualizarEnderecoResponse, cancellationToken);
                return new ResultDto<VendedorCreateResultDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar endereco."
                };
            }
            
            return new ResultDto<VendedorCreateResultDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status201Created,
                Dados = new VendedorCreateResultDto
                {
                    VendedorId = vendedorId.Value,
                    EnderecoId = enderecoId.Value
                }
            };
        }
    }
}
