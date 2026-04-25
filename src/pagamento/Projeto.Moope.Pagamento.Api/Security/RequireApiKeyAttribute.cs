using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.Pagamento.Api.Security
{
    public class RequireApiKeyAttribute : TypeFilterAttribute
    {
        public RequireApiKeyAttribute() : base(typeof(ApiKeyHeaderAuthFilter))
        {
        }
    }
}

