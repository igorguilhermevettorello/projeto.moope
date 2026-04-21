using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Plano;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class PlanoGetById : IPlanoGetById
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;

        public PlanoGetById(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis)
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
        }

        public async Task<ResultDto<PlanoDetailDto>> ExecutarAsync(Guid planoId, string? authorizationHeader, CancellationToken cancellationToken)
        {
            if (planoId == Guid.Empty)
            {
                return new ResultDto<PlanoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Id do plano é obrigatório.",
                    Dados = null
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = Utils.Combine(_apis.Plano, $"/api/plano/{planoId}");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            Utils.AplicarAutorizacao(request, authorizationHeader);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ResultDto<PlanoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Mensagem = "Plano nao encontrado.",
                    Dados = null
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var falhaDownstream = await Utils.FalhaDownstreamAsync(response, cancellationToken);
                return new ResultDto<PlanoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Falha ao acessar o servico Plano.",
                    Dados = null
                };
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var plano = await JsonSerializer.DeserializeAsync<PlanoDetailDto>(stream, JsonOptions, cancellationToken);

            if (plano == null)
            {
                return new ResultDto<PlanoDetailDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Resposta invalida do servico Plano.",
                    Dados = null
                };
            }
            
            return new ResultDto<PlanoDetailDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Mensagem = "Plano encontrado com sucesso.",
                Dados = plano
            };
        }
    }
}
