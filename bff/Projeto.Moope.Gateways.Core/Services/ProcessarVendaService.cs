using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.DTOs.Venda;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Models;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class ProcessarVendaService : IProcessarVendaService
    {
        /// <summary>Alinhado ao AutoMapper do monolito (CriarVendaDto -> CriarClienteCommand).</summary>
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
                    return new Result<VendaProcessingDto>
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
                return new Result<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel obter ou criar o cliente.",
                    Dados = null
                };
            }
            
            // Verificacao de plano:
            var planoResult = await _planoGetById.ExecutarAsync(request.PlanoId, authorizationHeader, cancellationToken);
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
            var vendaUrl = Utils.Combine(_apis.Pedido, "/api/pedido");
            var vendaBody = new
            {
                clienteId = clienteId.Value,
                vendedorId = vendedorIdEfetivoPedido == Guid.Empty ? (Guid?)null : vendedorIdEfetivoPedido,
                request.Email,
                request.NomeCliente,
                documento,
                planoId = request.PlanoId,
                request.Quantidade,
                request.Estado,
                descontos = request.Descontos ?? Array.Empty<string>(),
                tipoPessoa = (int)request.TipoPessoa,
                comodatoToken = request.ComodatoToken,
                cartao = new
                {
                    nome = request.NomeCartao,
                    numero = request.NumeroCartao,
                    cvv = request.Cvv,
                    validadeMmYy = request.DataValidade
                }
            };

            //using var vendaRequest = new HttpRequestMessage(HttpMethod.Post, vendaUrl);
            //AplicarAutorizacao(vendaRequest, authorizationHeader);
            //if (!string.IsNullOrWhiteSpace(idempotencyKey))
            //    vendaRequest.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
            //vendaRequest.Content = JsonContent.Create(vendaBody, options: JsonOptions);

            //using var vendaResponse = await httpClient.SendAsync(vendaRequest, cancellationToken);
            //if (!vendaResponse.IsSuccessStatusCode)
            //    return await FalhaDownstreamAsync(vendaResponse, cancellationToken);

            //await using var vendaStream = await vendaResponse.Content.ReadAsStreamAsync(cancellationToken);
            //var saida = await JsonSerializer.DeserializeAsync<ProcessarVendaOutputDto>(vendaStream, JsonOptions, cancellationToken);
            //if (saida == null)
            //{
            //    return new ProcessarVendaOrchestrationResult
            //    {
            //        Sucesso = false,
            //        StatusCode = StatusCodes.Status502BadGateway,
            //        CorpoErro = new { mensagem = "Resposta invalida do servico Venda." }
            //    };
            //}

            //// TODO: Salvar email de confirmacao + publicar filas emails / wpp_vendas (monolitico VendaController).

            //return new ProcessarVendaOrchestrationResult
            //{
            //    Sucesso = true,
            //    StatusCode = StatusCodes.Status200OK,
            //    Dados = new ProcessarVendaOutput
            //    {
            //        PlanoId = saida.PlanoId,
            //        Quantidade = saida.Quantidade,
            //        ValorTotal = saida.ValorTotal,
            //        VendaId = saida.VendaId,
            //        TransacaoId = saida.TransacaoId
            //    }
            //};

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

        //private static void AplicarAutorizacao(HttpRequestMessage request, string? authorizationHeader)
        //{
        //    if (!string.IsNullOrWhiteSpace(authorizationHeader))
        //        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        //}

        //private static async Task<Guid?> LerGuidDoCorpoJsonAsync(
        //    HttpResponseMessage response,
        //    CancellationToken cancellationToken)
        //{
        //    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        //    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        //    var root = doc.RootElement;

        //    if (root.ValueKind == JsonValueKind.String
        //        && Guid.TryParse(root.GetString(), out var guidFromString))
        //        return guidFromString;

        //    foreach (var name in new[] { "id", "Id" })
        //    {
        //        if (root.TryGetProperty(name, out var idProp))
        //        {
        //            if (idProp.ValueKind == JsonValueKind.String && Guid.TryParse(idProp.GetString(), out var g))
        //                return g;
        //            if (idProp.ValueKind == JsonValueKind.Object
        //                && idProp.TryGetProperty("id", out var nested)
        //                && nested.ValueKind == JsonValueKind.String
        //                && Guid.TryParse(nested.GetString(), out var nestedGuid))
        //                return nestedGuid;
        //        }
        //    }

        //    return null;
        //}

        //private static async Task<ProcessarVendaOrchestrationResult> FalhaDownstreamAsync(
        //    HttpResponseMessage response,
        //    CancellationToken cancellationToken)
        //{
        //    var status = (int)response.StatusCode;
        //    var texto = await response.Content.ReadAsStringAsync(cancellationToken);

        //    object corpo;
        //    if (string.IsNullOrWhiteSpace(texto))
        //        corpo = new { mensagem = response.ReasonPhrase };
        //    else
        //    {
        //        try
        //        {
        //            corpo = JsonSerializer.Deserialize<JsonElement>(texto, JsonOptions);
        //        }
        //        catch (JsonException)
        //        {
        //            corpo = texto;
        //        }
        //    }

        //    var statusNormalizado = status is >= 400 and <= 599
        //        ? status
        //        : StatusCodes.Status502BadGateway;

        //    return new ProcessarVendaOrchestrationResult
        //    {
        //        Sucesso = false,
        //        StatusCode = statusNormalizado,
        //        CorpoErro = corpo
        //    };
        //}

        

        //private sealed class ProcessarVendaOutputDto
        //{
        //    public Guid PlanoId { get; set; }

        //    public int Quantidade { get; set; }

        //    public decimal ValorTotal { get; set; }

        //    public Guid VendaId { get; set; }

        //    public Guid TransacaoId { get; set; }
        //}

        

        //private readonly record struct PlanoLookupResult(
        //    bool Sucesso,
        //    PlanoResponseDto? Plano,
        //    ProcessarVendaOrchestrationResult? Falha);

        //private async Task<string?> ObterBearerAuthAsync(HttpClient httpClient, CancellationToken cancellationToken)
        //{
        //    var url = Combine(_apis.Auth, "/api/auth/client/login");

        //    var raw = $"{_apis.AuthClientId}:{_apis.AuthClientSecret}";
        //    var basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));

        //    using var request = new HttpRequestMessage(HttpMethod.Post, url);
        //    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {basic}");

        //    using var response = await httpClient.SendAsync(request, cancellationToken);
        //    if (!response.IsSuccessStatusCode)
        //        return null;

        //    var json = await response.Content.ReadAsStringAsync(cancellationToken);
        //    if (string.IsNullOrWhiteSpace(json))
        //        return null;

        //    try
        //    {
        //        var node = JsonNode.Parse(json);
        //        var accessToken = node?["data"]?["accessToken"]?.GetValue<string>();
        //        if (string.IsNullOrWhiteSpace(accessToken))
        //            return null;

        //        return $"Bearer {accessToken}";
        //    }
        //    catch (JsonException)
        //    {
        //        return null;
        //    }
        //}
    }
}
