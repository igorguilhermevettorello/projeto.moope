namespace Projeto.Moope.Pagamento.Infrastructure.Services
{
    public interface ICelcoinTokenProvider
    {
        Task<string> GetAccessTokenAsync(string scope, CancellationToken cancellationToken = default);
        Task<(string AccessToken, int ExpiresInSeconds, string TokenType, string Scope)> AuthenticateAsync(string scope, CancellationToken cancellationToken = default);
    }
}

