using System.Text.Json;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Pagamento.Core.Services.Models;
using PagamentoModel = Projeto.Moope.Pagamento.Core.Models.Pagamento;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Services
{
    public interface IPagamentoService
    {
        Task<ResultDto<GatewayTokenDto>> AutenticarGatewayAsync(string scope, CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> CriarClienteAsync(CriarClienteGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> ListarClientesAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> CriarPlanoAsync(CriarPlanoGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> ListarPlanosAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> CriarCartaoAsync(CriarCartaoGatewayRequestDto request, CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> CriarAssinaturaComPlanoAsync(CriarAssinaturaComPlanoGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> CriarAssinaturaSemPlanoAsync(CriarAssinaturaSemPlanoGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> CriarAssinaturaManualAsync(CriarAssinaturaManualGatewayRequestDto request, CancellationToken cancellationToken = default);

        Task<ResultDto<PagamentoModel>> RegistrarPagamentoAssinaturaSemPlanoAsync(
            Guid clienteId,
            string gatewayCustomerId,
            string? gatewaySubscriptionId,
            string? gatewayPlanId,
            CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> AdicionarTransacaoEmAssinaturaAsync(AdicionarTransacaoEmAssinaturaGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> ListarTransacoesAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default);
        Task<ResultDto> CancelarTransacaoAsync(string transactionId, CancellationToken cancellationToken = default);

        Task<ResultDto<JsonElement>> CriarCobrancaAvulsaAsync(CriarCobrancaAvulsaGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<ResultDto<JsonElement>> ListarCobrancasAvulsasAsync(Dictionary<string, string?>? query, CancellationToken cancellationToken = default);
    }
}

