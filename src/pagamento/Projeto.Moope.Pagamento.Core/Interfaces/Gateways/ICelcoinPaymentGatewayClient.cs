using Projeto.Moope.Pagamento.Core.Services.Models;
using System.Text.Json;

namespace Projeto.Moope.Pagamento.Core.Interfaces.Gateways
{
    public interface ICelcoinPaymentGatewayClient
    {
        Task<JsonElement> AutenticarAsync(string scope, CancellationToken cancellationToken = default);

        Task<JsonElement> CriarClienteAsync(CriarClienteGatewayRequestDto request, CancellationToken cancellationToken = default);
        Task<JsonElement> ListarClientesAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);

        Task<JsonElement> CriarPlanoAsync(object request, CancellationToken cancellationToken = default);
        Task<JsonElement> ListarPlanosAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);

        Task<JsonElement> CriarCartaoAsync(string customerId, string typeId, object request, CancellationToken cancellationToken = default);

        Task<JsonElement> CriarAssinaturaAsync(object request, CancellationToken cancellationToken = default);
        Task<JsonElement> CriarAssinaturaManualAsync(object request, CancellationToken cancellationToken = default);

        Task<JsonElement> AdicionarTransacaoEmAssinaturaAsync(string subscriptionId, string typeId, object request, CancellationToken cancellationToken = default);
        Task<JsonElement> ListarTransacoesAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
        Task<JsonElement> CancelarTransacaoAsync(string transactionId, string typeId, CancellationToken cancellationToken = default);

        Task<JsonElement> CriarCobrancaAvulsaAsync(object request, CancellationToken cancellationToken = default);
        Task<JsonElement> ListarCobrancasAvulsasAsync(IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
    }
}

