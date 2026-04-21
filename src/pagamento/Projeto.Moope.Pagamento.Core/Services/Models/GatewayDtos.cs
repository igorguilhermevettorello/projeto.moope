namespace Projeto.Moope.Pagamento.Core.Services.Models
{
    public record GatewayTokenDto(
        string AccessToken,
        int ExpiresInSeconds,
        string TokenType,
        string Scope,
        DateTimeOffset ObtainedAtUtc
    );

    public record CriarClienteGatewayRequestDto(
        string Scope,
        object Payload,
        Guid? ClienteId = null
    );

    public record CriarPlanoGatewayRequestDto(
        string Scope,
        object Payload
    );

    public record CriarCartaoGatewayRequestDto(
        string Scope,
        string CustomerId,
        string TypeId,
        object Payload
    );

    public record CriarAssinaturaComPlanoGatewayRequestDto(
        string Scope,
        object Payload
    );

    public record CriarAssinaturaSemPlanoGatewayRequestDto(
        string Scope,
        object Payload
    );

    public record CriarAssinaturaManualGatewayRequestDto(
        string Scope,
        object Payload
    );

    public record AdicionarTransacaoEmAssinaturaGatewayRequestDto(
        string Scope,
        string SubscriptionId,
        string TypeId,
        object Payload
    );

    public record CriarCobrancaAvulsaGatewayRequestDto(
        string Scope,
        object Payload
    );
}

