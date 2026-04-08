using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Endereco.Api.Utils;
using System.Net.Http;
using System.Text;

namespace Projeto.Moope.Endereco.Api.Configurations
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
                // options.IncludeErrorDetails = hostEnvironment.IsDevelopment();
                options.IncludeErrorDetails = true;
                options.MapInboundClaims = true;

                if (jwtSettings.UseJwks)
                {
                    if (string.IsNullOrWhiteSpace(jwtSettings.Authority))
                    {
                        throw new InvalidOperationException("Jwt:Authority é obrigatório quando UseJwks é true.");
                    }

                    if (hostEnvironment.IsDevelopment())
                    {
                        options.BackchannelHttpHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                    }

                    var authority = jwtSettings.Authority.TrimEnd('/');
                    options.Authority = authority;
                    options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
                    options.Audience = jwtSettings.Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidIssuers = new[] { authority }
                    };
                }
                else
                {
                    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                }
            });

            return services;
        }
    }
}
