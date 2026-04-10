using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Gateways.Core.Models;
using Projeto.Moope.Gateways.Core.Options;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class CadastroClienteOrchestrator : ICadastroClienteOrchestrator
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public CadastroClienteOrchestrator(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<CadastroClienteOrchestrationResult> ExecutarAsync(
            CadastrarClienteInput request,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente))
            {
                return new CadastroClienteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    CorpoErro = new { mensagem = "DownstreamApis.Cliente nao configurado." }
                };
            }

            var clienteUrl = Combine(_apis.Cliente, "/api/cliente");
            var clienteBody = new
            {
                request.Nome,
                request.Email,
                request.TipoPessoa,
                request.CpfCnpj,
                request.Telefone,
                request.Ativo,
                Endereco = new
                {
                    request.Endereco.Cep,
                    request.Endereco.Logradouro,
                    Numero = request.Endereco.Numero ?? string.Empty,
                    Complemento = request.Endereco.Complemento ?? string.Empty,
                    request.Endereco.Bairro,
                    request.Endereco.Cidade,
                    request.Endereco.Estado
                },
                request.Senha,
                request.Confirmacao,
                request.NomeFantasia,
                request.InscricaoEstadual,
                request.VendedorId
            };

            var httpClient = _httpClientFactory.CreateClient();
            using var clienteRequest = new HttpRequestMessage(HttpMethod.Post, clienteUrl);
            AplicarAutorizacao(clienteRequest, authorizationHeader);
            clienteRequest.Content = JsonContent.Create(clienteBody, options: JsonOptions);

            using var clienteResponse = await httpClient.SendAsync(clienteRequest, cancellationToken);
            if (!clienteResponse.IsSuccessStatusCode)
                return await FalhaDownstreamAsync(clienteResponse, cancellationToken);

            var clienteId = await LerGuidRespostaAsync(clienteResponse, cancellationToken);
            if (clienteId == null)
            {
                return new CadastroClienteOrchestrationResult
                {
                    Sucesso = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    CorpoErro = new { mensagem = "Resposta invalida do servico Cliente (Id ausente)." }
                };
            }

            return new CadastroClienteOrchestrationResult
            {
                Sucesso = true,
                StatusCode = StatusCodes.Status201Created,
                Dados = new CadastrarClienteOutput
                {
                    ClienteId = clienteId.Value
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

        private static async Task<CadastroClienteOrchestrationResult> FalhaDownstreamAsync(
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

            return new CadastroClienteOrchestrationResult
            {
                Sucesso = false,
                StatusCode = statusNormalizado,
                CorpoErro = corpo
            };
        }
    }
}
