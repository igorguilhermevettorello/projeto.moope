using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Venda;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class ProcessarVendaService : IProcessarVendaService
    {
        private const string SenhaTemporariaClienteVenda = "ClienteVenda123!";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IClienteCreateService _clienteCreateService;
        private readonly IAuthClientLoginService _authClientLoginService;
        private readonly IPlanoGetById _planoGetById;

        public ProcessarVendaService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IClienteCreateService clienteCreateService,
            IAuthClientLoginService authClientLoginService,
            IPlanoGetById planoGetById)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _clienteCreateService = clienteCreateService;
            _authClientLoginService = authClientLoginService;
            _planoGetById = planoGetById;
        }

        public async Task<ResultDto<VendaProcessingDto>> ExecutarAsync(
            VendaCreateDto request,
            VendaUsuarioDto? usuario,
            string? authorizationHeader,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Pedido))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis (Cliente, Pedido) nao configurados.");
            }

            if (string.IsNullOrWhiteSpace(_apis.Plano))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:Plano nao configurado.");
            }

            if (string.IsNullOrWhiteSpace(_apis.ClienteApiKey))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:ClienteApiKey e obrigatorio para acessar o endpoint anonimo do BC Cliente.");
            }

            if (string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.AuthClientId)
                || string.IsNullOrWhiteSpace(_apis.AuthClientSecret))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis (Auth, AuthClientId, AuthClientSecret) sao obrigatorios para obter token via client/login.");
            }

            if (!string.IsNullOrWhiteSpace(request.CodigoCupom)
                && string.IsNullOrWhiteSpace(_apis.Vendedor))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:Vendedor e obrigatorio quando CodigoCupom e informado.");
            }

            var httpClient = _httpClientFactory.CreateClient();

            var rsAuth = await _authClientLoginService.ExecutarAsync(cancellationToken);
            if (rsAuth == null || !rsAuth.Status || string.IsNullOrWhiteSpace(rsAuth.Dados)) 
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    Mensagem = "Nao foi possivel obter token do servico Auth (client/login).",
                    Dados = null,
                };
            }
            var authBearerHeader = rsAuth.Dados;

            Guid? vendedorResolvido = request.VendedorId;
            if (!string.IsNullOrWhiteSpace(request.CodigoCupom))
            {
                var cupomUrl = Utils.Combine(_apis.Vendedor!, $"/api/vendedor/cupom/{Uri.EscapeDataString(request.CodigoCupom.Trim())}");
                using var cupomRequest = new HttpRequestMessage(HttpMethod.Get, cupomUrl);
                Utils.AplicarAutorizacao(cupomRequest, authorizationHeader);
                using var cupomResponse = await httpClient.SendAsync(cupomRequest, cancellationToken);

                if (cupomResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ResultDto<VendaProcessingDto>
                    {
                        Status = false,
                        Mensagem = "Cupom de vendedor invalido ou nao encontrado.",
                        Dados = null,
                    };
                }

                if (!cupomResponse.IsSuccessStatusCode) 
                {
                    var rs =  await Utils.FalhaDownstreamAsync(cupomResponse, cancellationToken);
                    return new ResultDto<VendaProcessingDto>
                    {
                        Status = false,
                        Mensagem = rs.Mensagem,
                        Dados = null,
                    };  
                }
                
                var cupomId = await Utils.LerGuidRespostaAsync(cupomResponse, cancellationToken);
                if (cupomId == null)
                {
                    return new ResultDto<VendaProcessingDto>
                    {
                        Status = false,
                        StatusCode = StatusCodes.Status502BadGateway,
                        Mensagem = "Resposta invalida do servico Vendedor (Id ausente).",
                        Dados = null,
                    };
                }

                vendedorResolvido = cupomId;
            }

            var vendedorIdEfetivoPedido = vendedorResolvido ?? Guid.Empty;

            var clienteId = await ClienteAsync(
                httpClient,
                request,
                usuario,
                vendedorResolvido,
                authBearerHeader,
                cancellationToken);

            if (clienteId is null)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel obter ou criar o cliente.",
                    Dados = null
                };
            }
            
            // Verificacao de plano:
            var planoResult = await _planoGetById.ExecutarAsync(request.PlanoId, authBearerHeader, cancellationToken);
            if (!planoResult.Status)
            { 
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Plano nao encontrado.",
                    Dados = null
                };
            }
            
            var plano = planoResult.Dados;
            if (!plano.Status)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Plano inativo.",
                    Dados = null
                };
            }

            var documento = string.IsNullOrWhiteSpace(request.CpfCnpj)
                ? string.Empty
                : Regex.Replace(request.CpfCnpj, @"\D", "");

            // Pedido (Venda.Api) orquestra pagamento internamente; o BFF apenas envia o payload completo.
            var pedidoUrl = Utils.Combine(_apis.Pedido, "/api/pedido");
            var pedidoBody = new
            {
                ClienteId = clienteId.Value,
                VendedorId = vendedorIdEfetivoPedido == Guid.Empty ? (Guid?)null : vendedorIdEfetivoPedido,
                request.PlanoId,
                request.Quantidade,
                request.TipoPessoa,
                request.Estado
            };

            using var pedidoRequest = new HttpRequestMessage(HttpMethod.Post, pedidoUrl);
            Utils.AplicarAutorizacao(pedidoRequest, authBearerHeader);
            pedidoRequest.Content = JsonContent.Create(pedidoBody, options: JsonOptions);

            using var pedidoResponse = await httpClient.SendAsync(pedidoRequest, cancellationToken);
            if (!pedidoResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(pedidoResponse, cancellationToken);
                return new ResultDto<VendaProcessingDto>    
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar venda."
                };
            }



            return new ResultDto<VendaProcessingDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Mensagem = null,
                Dados = new VendaProcessingDto
                {
                    teste = "ok"
                }
            };
        }

        private async Task<Guid?> ClienteAsync(
            HttpClient httpClient,
            VendaCreateDto request,
            VendaUsuarioDto? usuario,
            Guid? vendedorCupom,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            var trimmedEmail = request.Email.Trim();
            var encodedEmail = Uri.EscapeDataString(trimmedEmail);
            var buscaUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/email?email={encodedEmail}");

            using (var getRequest = new HttpRequestMessage(HttpMethod.Get, buscaUrl))
            {
                Utils.AplicarAutorizacao(getRequest, authorizationHeader);
                getRequest.Headers.TryAddWithoutValidation("x-api-key", _apis.ClienteApiKey.Trim());
                using var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
                if (getResponse.IsSuccessStatusCode)
                {
                    var id = await Utils.LerGuidDoCorpoJsonAsync(getResponse, cancellationToken);
                    if (id != null)
                        return id;
                }
                else if (getResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            Guid? vendedorId;
            if (usuario?.IsAdministrador == true)
                vendedorId = vendedorCupom;
            else if (usuario?.UsuarioId is { } uid && uid != Guid.Empty)
                vendedorId = uid;
            else
                vendedorId = vendedorCupom;

            var clienteCreateDto = new ClienteCreateDto
            {
                Nome = request.NomeCliente,
                Email = request.Email,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                Telefone = request.Telefone,
                Ativo = true,
                Senha = SenhaTemporariaClienteVenda,
                Confirmacao = SenhaTemporariaClienteVenda,
                NomeFantasia = request.NomeCliente,
                InscricaoEstadual = string.Empty,
                VendedorId = vendedorId
            };

            var rs = await _clienteCreateService.ExecutarAsync(clienteCreateDto, authorizationHeader, cancellationToken);

            if (!rs.Status) return null;

            return rs.Dados?.ClienteId;
        }
    }
}
