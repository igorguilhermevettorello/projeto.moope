using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Cartao;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.GalaxPay;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.GalaxPay
{
    public class CartaoGalaxPayCreateService : ICartaoGalaxPayCreateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public CartaoGalaxPayCreateService(IHttpClientFactory httpClientFactory, IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public Task<ResultDto<CartaoGalaxPayDetailDto>> ExecutarAsync(CartaoGalaxPayCreateDto request, CancellationToken cancellationToken)
        {
            return ExecutarCoreAsync(request, cancellationToken);
        }

        private async Task<ResultDto<CartaoGalaxPayDetailDto>> ExecutarCoreAsync(
            CartaoGalaxPayCreateDto request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Pagamento))
                return Utils.FalhaConfig<CartaoGalaxPayDetailDto>("DownstreamApis:Pagamento nao configurado.");

            var url = Utils.Combine(_apis.Pagamento, "/api/pagamento/cartoes");

            var body = new
            {
                CustomerId = request.CustomerId.ToString(),
                TypeId = "galaxPayId",
                Number = request.Number,
                Holder = request.Holder,
                ExpiresAt = request.ExpiresAt,
                Cvv = request.Cvv
            };

            var httpClient = _httpClientFactory.CreateClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };

            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<CartaoGalaxPayDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar cartao no Pagamento."
                };
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
            if (json.ValueKind == JsonValueKind.Undefined)
            {
                return new ResultDto<CartaoGalaxPayDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Pagamento (corpo ausente)."
                };
            }

            var galaxPayStr = TryGetStringProperty(json, "galaxPay");
            if (string.IsNullOrWhiteSpace(galaxPayStr) || !int.TryParse(galaxPayStr, out var galaxPayId))
            {
                return new ResultDto<CartaoGalaxPayDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Dados = null,
                    Mensagem = "Resposta invalida do servico Pagamento (galaxPay nao retornado ou invalido)."
                };
            }

            return new ResultDto<CartaoGalaxPayDetailDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = new CartaoGalaxPayDetailDto
                {
                    GalaxPayId = galaxPayId
                }
            };
        }

        private static string? TryGetStringProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var prop in element.EnumerateObject())
            {
                if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (prop.Value.ValueKind == JsonValueKind.String)
                    return prop.Value.GetString();

                var s = prop.Value.ToString();
                return string.IsNullOrWhiteSpace(s) ? null : s;
            }

            return null;
        }
    }
}
