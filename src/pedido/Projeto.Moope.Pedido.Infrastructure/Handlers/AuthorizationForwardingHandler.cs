using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Projeto.Moope.Pedido.Infrastructure.Helpers;

namespace Projeto.Moope.Pedido.Infrastructure.Handlers
{
    public sealed class AuthorizationForwardingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationForwardingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var token = await httpContext.GetTokenAsync("access_token");
                var authorizationHeader = !string.IsNullOrWhiteSpace(token)
                    ? $"Bearer {token}"
                    : httpContext.Request.Headers.Authorization.ToString();

                HttpAuthorizationHelper.Aplicar(request, authorizationHeader);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
