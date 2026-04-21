using Microsoft.AspNetCore.Http;
using Projeto.Moope.Core.DTOs;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Helpers
{
    public static class Utils
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        public static string Combine(string baseUrl, string path)
        {
            return $"{baseUrl.TrimEnd('/')}{path}";
        }

        public  static void AplicarAutorizacao(HttpRequestMessage request, string? authorizationHeader)
        {
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
                request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        public static async Task<Guid?> LerGuidRespostaAsync(
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

        public static async Task<ResultDto> FalhaDownstreamAsync(
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

            return new ResultDto
            {
                Status = false,
                StatusCode = statusNormalizado,
                Mensagem = corpo?.ToString() ?? "Erro desconhecido"
            };
        }

        public static async Task<Guid?> LerGuidDoCorpoJsonAsync(
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

        public static ResultDto<T> FalhaConfig<T>(string mensagem)
        {
            return new ResultDto<T>
            {
                Status = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Mensagem = mensagem,
                Dados = default(T)
            };
        }
    }
}
