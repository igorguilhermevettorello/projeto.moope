using System.Net.Http;

namespace Projeto.Moope.Vendedor.Api.Utils
{
    public sealed class LoggingHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"[JWKS] Request: {request.Method} {request.RequestUri}");

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine($"[JWKS] Response: {(int)response.StatusCode} {response.StatusCode}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JWKS] Error: {ex}");
                throw;
            }
        }
    }
}
