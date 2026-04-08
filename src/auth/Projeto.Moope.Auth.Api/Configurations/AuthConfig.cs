using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Auth.Api.Services;
using Projeto.Moope.Auth.Api.Utils;
using System.Text;

namespace Projeto.Moope.Auth.Api.Configurations
{
    public static class AuthConfig
    {
        public static IServiceCollection AddAuthConfig(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            var jwtSettingsSection = configuration.GetSection("Jwt");
            services.Configure<JwtSettings>(jwtSettingsSection);
            services.AddSingleton<IJwtSigningKeyProvider, JwtSigningKeyProvider>();

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>()
                ?? throw new InvalidOperationException("Seção de configuração 'Jwt' é obrigatória.");

            var requireHttpsMetadata = !hostEnvironment.IsDevelopment();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.SaveToken = true;
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience
                };
            });

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IJwtSigningKeyProvider>((options, signingKeys) =>
                {
                    options.TokenValidationParameters.IssuerSigningKey = signingKeys.GetIssuerSigningKey();
                });

            return services;
        }
    }
}
