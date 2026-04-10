using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.Models;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class ProcessarVendaOrchestrator : IProcessarVendaOrchestrator
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

        public ProcessarVendaOrchestrator(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ProcessarVendaOrchestrationResult> ExecutarAsync(
            ProcessarVendaInput request,
            ProcessarVendaUsuarioContext? usuario,
            string? authorizationHeader,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Pagamento)
                || string.IsNullOrWhiteSpace(_apis.Venda))
            {
                return FalhaConfig("DownstreamApis (Cliente, Pagamento, Venda) nao configurados.");
            }

            if (!string.IsNullOrWhiteSpace(request.CodigoCupom)
                && string.IsNullOrWhiteSpace(_apis.Vendedor))
            {
                return FalhaConfig("DownstreamApis:Vendedor e obrigatorio quando CodigoCupom e informado.");
            }

            var httpClient = _httpClientFactory.CreateClient();

            Guid? vendedorResolvido = request.VendedorId;
            if (!string.IsNullOrWhiteSpace(request.CodigoCupom))
            {
                var cupomUrl = Combine(_apis.Vendedor!, $"/api/vendedor/cupom/{Uri.EscapeDataString(request.CodigoCupom.Trim())}");
                using var cupomRequest = new HttpRequestMessage(HttpMethod.Get, cupomUrl);
                AplicarAutorizacao(cupomRequest, authorizationHeader);
                using var cupomResponse = await httpClient.SendAsync(cupomRequest, cancellationToken);

                if (cupomResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ProcessarVendaOrchestrationResult
                    {
                        Sucesso = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        CorpoErro = new[]
                        {
                            new
                            {
                                campo = "CodigoCupom",
                                mensagem = "Cupom de vendedor invalido ou nao encontrado."
                            }
                        }
                    };
                }

                if (!cupomResponse.IsSuccessStatusCode)
                    return await FalhaDownstreamAsync(cupomResponse, cancellationToken);

                var cupomId = await LerGuidDoCorpoJsonAsync(cupomResponse, cancellationToken);
                if (cupomId == null)
                {
                    return new ProcessarVendaOrchestrationResult
                    {
                        Sucesso = false,
                        StatusCode = StatusCodes.Status502BadGateway,
                        CorpoErro = new { mensagem = "Resposta invalida do servico Vendedor (Id ausente)." }
                    };
                }

                vendedorResolvido = cupomId;
            }

            var vendedorIdEfetivoPedido = vendedorResolvido ?? Guid.Empty;

            var clienteId = await GarantirClienteAsync(
                httpClient,
                request,
                usuario,
                vendedorResolvido,
                authorizationHeader,
                cancellationToken);

            if (clienteId is null)
                return new ProcessarVendaOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Nao foi possivel obter ou criar o cliente." }
                };

            var documento = string.IsNullOrWhiteSpace(request.CpfCnpj)
                ? string.Empty
                : Regex.Replace(request.CpfCnpj, @"\D", "");

            var pagamentoUrl = Combine(_apis.Pagamento, "/api/pagamento/venda/executar");
            var pagamentoBody = new
            {
                clienteInternoId = clienteId.Value,
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

            using var pagamentoRequest = new HttpRequestMessage(HttpMethod.Post, pagamentoUrl);
            AplicarAutorizacao(pagamentoRequest, authorizationHeader);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                pagamentoRequest.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
            pagamentoRequest.Content = JsonContent.Create(pagamentoBody, options: JsonOptions);

            using var pagamentoResponse = await httpClient.SendAsync(pagamentoRequest, cancellationToken);
            if (!pagamentoResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(pagamentoResponse, cancellationToken);

            var pagamentoJson = await pagamentoResponse.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(pagamentoJson))
            {
                return new ProcessarVendaOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta vazia do servico Pagamento." }
                };
            }

            JsonNode? paymentNode;
            try
            {
                paymentNode = JsonNode.Parse(pagamentoJson);
            }
            catch (JsonException ex)
            {
                return new ProcessarVendaOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "JSON invalido do servico Pagamento.", detalhe = ex.Message }
                };
            }

            var vendaUrl = Combine(_apis.Venda, "/api/venda/processar");
            var vendaBody = new JsonObject
            {
                ["clienteId"] = clienteId.Value.ToString(),
                ["planoId"] = request.PlanoId.ToString(),
                ["quantidade"] = request.Quantidade,
                ["tipoPessoa"] = (int)request.TipoPessoa,
                ["nomeCliente"] = request.NomeCliente,
                ["email"] = request.Email,
                ["documento"] = documento,
                ["descontos"] = JsonSerializer.SerializeToNode(request.Descontos ?? Array.Empty<string>()) ?? new JsonArray(),
                ["resultadoPagamento"] = paymentNode
            };

            if (vendedorIdEfetivoPedido != Guid.Empty)
                vendaBody["vendedorId"] = vendedorIdEfetivoPedido.ToString();
            if (!string.IsNullOrWhiteSpace(request.Estado))
                vendaBody["estado"] = request.Estado;
            if (!string.IsNullOrWhiteSpace(request.ComodatoToken))
                vendaBody["comodatoToken"] = request.ComodatoToken;

            using var vendaRequest = new HttpRequestMessage(HttpMethod.Post, vendaUrl);
            AplicarAutorizacao(vendaRequest, authorizationHeader);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                vendaRequest.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
            vendaRequest.Content = new StringContent(
                vendaBody.ToJsonString(),
                System.Text.Encoding.UTF8,
                "application/json");

            using var vendaResponse = await httpClient.SendAsync(vendaRequest, cancellationToken);
            if (!vendaResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(vendaResponse, cancellationToken);

            await using var vendaStream = await vendaResponse.Content.ReadAsStreamAsync(cancellationToken);
            var saida = await JsonSerializer.DeserializeAsync<ProcessarVendaOutputDto>(vendaStream, JsonOptions, cancellationToken);
            if (saida == null)
            {
                return new ProcessarVendaOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta invalida do servico Venda." }
                };
            }

            // TODO: Salvar email de confirmacao + publicar filas emails / wpp_vendas (monolitico VendaController).

            return new ProcessarVendaOrchestrationResult
            {
                Sucesso = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = new ProcessarVendaOutput
                {
                    PlanoId = saida.PlanoId,
                    Quantidade = saida.Quantidade,
                    ValorTotal = saida.ValorTotal,
                    VendaId = saida.VendaId,
                    TransacaoId = saida.TransacaoId
                }
            };
        }

        private async Task<Guid?> GarantirClienteAsync(
            HttpClient httpClient,
            ProcessarVendaInput request,
            ProcessarVendaUsuarioContext? usuario,
            Guid? vendedorResolvidoCupom,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            var encodedEmail = Uri.EscapeDataString(request.Email.Trim());
            var buscaUrl = Combine(_apis.Cliente, $"/api/cliente/email/{encodedEmail}");

            using (var getRequest = new HttpRequestMessage(HttpMethod.Get, buscaUrl))
            {
                AplicarAutorizacao(getRequest, authorizationHeader);
                using var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
                if (getResponse.IsSuccessStatusCode)
                {
                    var id = await LerGuidDoCorpoJsonAsync(getResponse, cancellationToken);
                    if (id != null)
                        return id;
                }
                else if (getResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            Guid? vendedorIdCriacao;
            if (usuario?.IsAdministrador == true)
                vendedorIdCriacao = vendedorResolvidoCupom;
            else if (usuario?.UsuarioId is { } uid && uid != Guid.Empty)
                vendedorIdCriacao = uid;
            else
                vendedorIdCriacao = vendedorResolvidoCupom;

            var criarUrl = Combine(_apis.Cliente, "/api/cliente");
            var criarBody = new
            {
                nome = request.NomeCliente,
                email = request.Email.Trim(),
                cpfCnpj = request.CpfCnpj ?? string.Empty,
                telefone = request.Telefone,
                tipoPessoa = (int)request.TipoPessoa,
                ativo = true,
                senha = SenhaTemporariaClienteVenda,
                confirmacao = SenhaTemporariaClienteVenda,
                vendedorId = vendedorIdCriacao
            };

            using var postRequest = new HttpRequestMessage(HttpMethod.Post, criarUrl);
            AplicarAutorizacao(postRequest, authorizationHeader);
            postRequest.Content = JsonContent.Create(criarBody, options: JsonOptions);

            using var postResponse = await httpClient.SendAsync(postRequest, cancellationToken);
            if (!postResponse.IsSuccessStatusCode)
                return null;

            return await LerGuidDoCorpoJsonAsync(postResponse, cancellationToken);
        }

        private static ProcessarVendaOrchestrationResult FalhaConfig(string mensagem)
        {
            return new ProcessarVendaOrchestrationResult
            {
                Sucesso = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                CorpoErro = new { mensagem }
            };
        }

        private static string Combine(string baseUrl, string path)
        {
            return $"{baseUrl.TrimEnd('/')}{path}";
        }

        private static void AplicarAutorizacao(HttpRequestMessage request, string? authorizationHeader)
        {
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
                request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        private static async Task<Guid?> LerGuidDoCorpoJsonAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String
                && Guid.TryParse(root.GetString(), out var guidFromString))
                return guidFromString;

            foreach (var name in new[] { "id", "Id" })
            {
                if (root.TryGetProperty(name, out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.String && Guid.TryParse(idProp.GetString(), out var g))
                        return g;
                    if (idProp.ValueKind == JsonValueKind.Object
                        && idProp.TryGetProperty("id", out var nested)
                        && nested.ValueKind == JsonValueKind.String
                        && Guid.TryParse(nested.GetString(), out var nestedGuid))
                        return nestedGuid;
                }
            }

            return null;
        }

        private static async Task<ProcessarVendaOrchestrationResult> FalhaDownstreamAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var status = (int)response.StatusCode;
            var texto = await response.Content.ReadAsStringAsync(cancellationToken);

            object corpo;
            if (string.IsNullOrWhiteSpace(texto))
                corpo = new { mensagem = response.ReasonPhrase };
            else
            {
                try
                {
                    corpo = JsonSerializer.Deserialize<JsonElement>(texto, JsonOptions);
                }
                catch (JsonException)
                {
                    corpo = texto;
                }
            }

            var statusNormalizado = status is >= 400 and <= 599
                ? status
                : StatusCodes.Status502BadGateway;

            return new ProcessarVendaOrchestrationResult
            {
                Sucesso = false,
                StatusCode = statusNormalizado,
                CorpoErro = corpo
            };
        }

        private sealed class ProcessarVendaOutputDto
        {
            public Guid PlanoId { get; set; }

            public int Quantidade { get; set; }

            public decimal ValorTotal { get; set; }

            public Guid VendaId { get; set; }

            public Guid TransacaoId { get; set; }
        }
    }
}
