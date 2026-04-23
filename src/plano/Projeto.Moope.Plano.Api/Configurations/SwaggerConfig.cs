namespace Projeto.Moope.Plano.Api.Configurations
{
    public static class SwaggerConfig
    {
        public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app)
        {
            app.UseMiddleware<SwaggerAuthorizedMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();
            return app;
        }
    }

    public class SwaggerAuthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string swaggerAuth = context.Request.Cookies["SwaggerAuth"];

            if (context.Request.Path.StartsWithSegments("/swagger")
                && string.IsNullOrEmpty(swaggerAuth))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Redirect("/api/swagger-auth");
                return;
            }
            else if (context.Request.Path.StartsWithSegments("/swagger")
                && !string.IsNullOrEmpty(swaggerAuth)
                && !swaggerAuth.Equals("true"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Redirect("/api/swagger-auth");
                return;
            }

            await _next.Invoke(context);
        }
    }
}

