using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Projeto.Moope.Cliente.Api.Configurations
{
    public sealed class ApiKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requiresApiKey = context.MethodInfo
                .GetCustomAttributes(typeof(ApiKeyRequiredAttribute), inherit: true)
                .Any();

            if (!requiresApiKey)
                return;

            operation.Parameters ??= new List<OpenApiParameter>();

            var alreadyHas = operation.Parameters.Any(p =>
                string.Equals(p.Name, "x-api-key", StringComparison.OrdinalIgnoreCase) &&
                p.In == ParameterLocation.Header);

            if (alreadyHas)
                return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-api-key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "API key para endpoints anônimos",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
