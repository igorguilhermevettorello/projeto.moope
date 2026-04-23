using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.Comodato.Api.Configurations
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsDevelopmentPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsStagingPolicy", policy =>
                {
                    policy
                        .WithOrigins("https://staging-compre.moope.com.br")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsProductionPolicy", policy =>
                {
                    policy
                        .WithOrigins("https://compre.moope.com.br")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}

