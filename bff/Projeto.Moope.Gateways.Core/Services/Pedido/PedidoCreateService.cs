using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido;
using Projeto.Moope.Gateways.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.Pedido
{
    public class PedidoCreateService : IPedidoCreateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public PedidoCreateService(IHttpClientFactory httpClientFactory, IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }
        public async Task<ResultDto<PedidoDetailDto>> ExecutarAsync(PedidoCreateDto request, string? authBearerHeader, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.Endereco))
            {
                return new ResultDto<PedidoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "DownstreamApis (Cliente, Auth, Endereco) nao configurados."
                };
            }

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                return new ResultDto<PedidoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Dados = null,
                    Mensagem = "Header Idempotency-Key e obrigatorio."
                };
            }

            var httpClient = _httpClientFactory.CreateClient();

            // Pedido (Venda.Api) orquestra pagamento internamente; o BFF apenas envia o payload completo.
            var pedidoUrl = Utils.Combine(_apis.Pedido, "/api/pedido");
            var pedidoBody = new
            {
                request.ClienteId,
                request.VendedorId,
                request.PlanoId,
                request.Quantidade,
                request.TipoPessoa,
                request.Estado
            };

            using var pedidoRequest = new HttpRequestMessage(HttpMethod.Post, pedidoUrl);
            Utils.AplicarAutorizacao(pedidoRequest, authBearerHeader);
            pedidoRequest.Headers.TryAddWithoutValidation("Idempotency-Key", request.IdempotencyKey);
            pedidoRequest.Content = JsonContent.Create(pedidoBody, options: JsonOptions);

            using var pedidoResponse = await httpClient.SendAsync(pedidoRequest, cancellationToken);
            if (!pedidoResponse.IsSuccessStatusCode)
            {
                var rs = await Utils.FalhaDownstreamAsync(pedidoResponse, cancellationToken);
                return new ResultDto<PedidoDetailDto>
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Dados = null,
                    Mensagem = rs.Mensagem ?? "Erro desconhecido ao criar pedido."
                };
            }

            var pedidoId = await Utils.LerGuidDoCorpoJsonAsync(pedidoResponse, cancellationToken);
            if (pedidoId == null)
            {
                return new ResultDto<PedidoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Dados = null,
                    Mensagem = "Resposta do Pedido.Api nao continha o ID do pedido criado."
                };
            }

            return new ResultDto<PedidoDetailDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status201Created,
                Dados = new PedidoDetailDto { Id = pedidoId.Value },
                Mensagem = "Pedido criado com sucesso."
            };
        }
    }
}
