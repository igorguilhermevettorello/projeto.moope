using System.Net.Http.Headers;

namespace Projeto.Moope.Pedido.Infrastructure.Helpers
{
    public static class HttpAuthorizationHelper
    {
        public static void Aplicar(HttpRequestMessage request, string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return;

            var valor = authorizationHeader.Trim();
            if (!valor.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                valor = $"Bearer {valor}";

            if (AuthenticationHeaderValue.TryParse(valor, out var auth))
                request.Headers.Authorization = auth;
        }
    }
}
