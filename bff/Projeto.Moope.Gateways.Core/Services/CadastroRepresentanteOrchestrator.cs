using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.Models;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class CadastroRepresentanteOrchestrator : ICadastroRepresentanteOrchestrator
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public CadastroRepresentanteOrchestrator(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<CadastroRepresentanteOrchestrationResult> ExecutarAsync(
            CadastrarRepresentanteInput request,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Vendedor)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new CadastroRepresentanteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    CorpoErro = new { mensagem = "DownstreamApis (Vendedor, Auth, Endereco) nao configurados." }
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            var authUrl = Combine(_apis.Auth, "/api/usuario");
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
            AplicarAutorizacao(usuarioRequest, authorizationHeader);
            usuarioRequest.Content = JsonContent.Create(usuarioBody, options: JsonOptions);

            using var usuarioResponse = await httpClient.SendAsync(usuarioRequest, cancellationToken);
            if (!usuarioResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(usuarioResponse, cancellationToken);

            var usuarioId = await LerGuidRespostaAsync(usuarioResponse, cancellationToken);
            if (usuarioId == null)
            {
                return new CadastroRepresentanteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta invalida do servico Auth (Id ausente)." }
                };
            }

            var vendedorUrl = Combine(_apis.Vendedor, "/api/vendedor");
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
            AplicarAutorizacao(vendedorRequest, authorizationHeader);
            vendedorRequest.Content = JsonContent.Create(vendedorBody, options: JsonOptions);
            using var vendedorResponse = await httpClient.SendAsync(vendedorRequest, cancellationToken);

            if (!vendedorResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(vendedorResponse, cancellationToken);

            var vendedorId = await LerGuidRespostaAsync(vendedorResponse, cancellationToken);
            if (vendedorId == null)
            {
                return new CadastroRepresentanteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta invalida do servico Vendedor (Id ausente)." }
                };
            }



            var enderecoUrl = Combine(_apis.Endereco, "/api/endereco");
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
            AplicarAutorizacao(enderecoRequest, authorizationHeader);
            enderecoRequest.Content = JsonContent.Create(enderecoBody, options: JsonOptions);

            using var enderecoResponse = await httpClient.SendAsync(enderecoRequest, cancellationToken);
            if (!enderecoResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(enderecoResponse, cancellationToken);

            var enderecoId = await LerGuidRespostaAsync(enderecoResponse, cancellationToken);
            if (enderecoId == null)
            {
                return new CadastroRepresentanteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta invalida do servico Endereco (Id ausente)." }
                };
            }

            var atualizarEnderecoUrl = Combine(_apis.Auth, $"/api/usuario/{usuarioId}/endereco/{enderecoId}");
            using var atualizarEnderecoRequest = new HttpRequestMessage(HttpMethod.Patch, atualizarEnderecoUrl);
            AplicarAutorizacao(atualizarEnderecoRequest, authorizationHeader);

            using var atualizarEnderecoResponse = await httpClient.SendAsync(atualizarEnderecoRequest, cancellationToken);
            if (!atualizarEnderecoResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(atualizarEnderecoResponse, cancellationToken);

            return new CadastroRepresentanteOrchestrationResult
            {
                Sucesso = true,
                StatusCode = StatusCodes.Status201Created,
                Dados = new CadastroRepresentanteCompostoOutput
                {
                    VendedorId = vendedorId.Value,
                    UsuarioId = usuarioId.Value,
                    EnderecoId = enderecoId.Value
                }
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

        private static async Task<Guid?> LerGuidRespostaAsync(
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
                if (root.TryGetProperty(name, out var idProp)
                    && idProp.ValueKind == JsonValueKind.String
                    && Guid.TryParse(idProp.GetString(), out var g))
                    return g;
            }

            return null;
        }

        private static async Task<CadastroRepresentanteOrchestrationResult> FalhaDownstreamAsync(
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

            return new CadastroRepresentanteOrchestrationResult
            {
                Sucesso = false,
                StatusCode = statusNormalizado,
                CorpoErro = corpo
            };
        }
    }
}
