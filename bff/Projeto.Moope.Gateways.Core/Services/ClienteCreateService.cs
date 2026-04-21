using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class ClienteCreateService : IClienteCreateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public ClienteCreateService(IHttpClientFactory httpClientFactory, IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<ClienteCreateResultDto>> ExecutarAsync(ClienteCreateDto request, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new ResultDto<ClienteCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Cliente, Auth, Endereco) nao configurados."
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
                TipoUsuario = TipoUsuario.Cliente,
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
                return new ResultDto<ClienteCreateResultDto>
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
                return new ResultDto<ClienteCreateResultDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Auth (Id ausente)."
                };
            }

            var clienteUrl = Utils.Combine(_apis.Cliente, "/api/cliente");
            var clienteBody = new
            {
                request.Telefone,
                request.VendedorId,
                UsuarioId = (Guid)usuarioId
            };

            using var clienteRequest = new HttpRequestMessage(HttpMethod.Post, clienteUrl);
            Utils.AplicarAutorizacao(clienteRequest, authorizationHeader);
            clienteRequest.Content = JsonContent.Create(clienteBody, options: JsonOptions);

            using var clienteResponse = await httpClient.SendAsync(clienteRequest, cancellationToken);
            if (!clienteResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(clienteResponse, cancellationToken);
                return new ResultDto<ClienteCreateResultDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar cliente."
                };  
            }
            
            var clienteId = await Utils.LerGuidRespostaAsync(clienteResponse, cancellationToken);
            if (clienteId == null)
            {
                return new ResultDto<ClienteCreateResultDto>    
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Cliente (Id ausente)."
                };
            }

            Guid? enderecoId = null;
            if (request.Endereco != null)
            {
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
                    return new ResultDto<ClienteCreateResultDto>
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Dados = null,
                        Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar endereco."
                    };
                }


                enderecoId = await Utils.LerGuidRespostaAsync(enderecoResponse, cancellationToken);
                if (enderecoId == null)
                {
                    return new ResultDto<ClienteCreateResultDto>
                    {
                        Status = false,
                        StatusCode = StatusCodes.Status502BadGateway,
                        Dados = null,
                        Mensagem = "Resposta invalida do servico Endereco (Id ausente)."
                    };
                }

                var atualizarEnderecoUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/{clienteId}/endereco/{enderecoId}");
                using var atualizarEnderecoRequest = new HttpRequestMessage(HttpMethod.Patch, atualizarEnderecoUrl);
                Utils.AplicarAutorizacao(atualizarEnderecoRequest, authorizationHeader);

                using var atualizarEnderecoResponse = await httpClient.SendAsync(atualizarEnderecoRequest, cancellationToken);
                if (!atualizarEnderecoResponse.IsSuccessStatusCode)
                {
                    var rs = await Utils.FalhaDownstreamAsync(atualizarEnderecoResponse, cancellationToken);
                    return new ResultDto<ClienteCreateResultDto>
                    {
                        Status = false,
                        StatusCode = rs.StatusCode,
                        Dados = null,
                        Mensagem = rs.Mensagem ?? "Erro desconhecido ao atualizar endereco."
                    };
                }
            }
            
            return new ResultDto<ClienteCreateResultDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status201Created,
                Dados = new ClienteCreateResultDto
                {
                    ClienteId = clienteId.Value,
                    EnderecoId = enderecoId.Value
                }
            };
        }
    }
}
