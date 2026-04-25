using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Projeto.Moope.Pagamento.Api.Configurations;

namespace Projeto.Moope.Pagamento.Api.Security
{
    public class ApiKeyHeaderAuthFilter : IAsyncActionFilter
    {
        private const string HeaderName = "X-Api-Key";
        private readonly IdempotencyKeyGeneratorOptions _options;

        public ApiKeyHeaderAuthFilter(IOptions<IdempotencyKeyGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var providedKey = provided.ToString();
            if (!ApiKeysEqual(_options.ApiKey, providedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }

        private static bool ApiKeysEqual(string expected, string provided)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var providedBytes = Encoding.UTF8.GetBytes(provided);

            return expectedBytes.Length == providedBytes.Length
                   && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
    }
}

