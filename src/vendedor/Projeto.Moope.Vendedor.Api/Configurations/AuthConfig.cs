using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Vendedor.Api.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Projeto.Moope.Vendedor.Api.Configurations
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
                ?? throw new InvalidOperationException("Seção Jwt é obrigatória.");

            var requireHttpsMetadata = !hostEnvironment.IsDevelopment();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = requireHttpsMetadata;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;
                    options.MapInboundClaims = true;

                    if (jwtSettings.UseJwks)
                    {
                        if (string.IsNullOrWhiteSpace(jwtSettings.Authority))
                            throw new InvalidOperationException("Jwt:Authority é obrigatório quando UseJwks = true.");

                        var authority = jwtSettings.Authority.TrimEnd('/');
                        var metadataAddress = $"{authority}/.well-known/openid-configuration";

                        HttpMessageHandler backchannelHandler;

                        if (hostEnvironment.IsDevelopment())
                        {
                            backchannelHandler = new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                            };
                        }
                        else
                        {
                            backchannelHandler = new HttpClientHandler();
                        }

                        options.BackchannelHttpHandler = backchannelHandler;
                        options.Authority = authority;
                        options.MetadataAddress = metadataAddress;
                        options.Audience = jwtSettings.Audience;

                        var httpClient = new HttpClient(backchannelHandler);
                        var configurationManager =
                            new ConfigurationManager<OpenIdConnectConfiguration>(
                                metadataAddress,
                                new OpenIdConnectConfigurationRetriever(),
                                new HttpDocumentRetriever(httpClient)
                                {
                                    RequireHttps = requireHttpsMetadata
                                });

                        var jwksUrl = $"{authority}/.well-known/jwks.json";

                        HttpMessageHandler handler = hostEnvironment.IsDevelopment()
                            ? new LoggingHandler
                            {
                                ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                            }
                            : new LoggingHandler();

                        options.BackchannelHttpHandler = handler;

                        options.ConfigurationManager = configurationManager;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidAudience = jwtSettings.Audience,
                            ValidIssuers = new[]
                            {
                                authority,
                                jwtSettings.Issuer?.TrimEnd('/') ?? authority
                            },

                            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                            {
                                using var http = new HttpClient(handler, disposeHandler: false);
                                var jwksJson = http.GetStringAsync(jwksUrl).GetAwaiter().GetResult();

                                var jwks = new JsonWebKeySet(jwksJson);
                                var keys = jwks.GetSigningKeys();

                                if (!string.IsNullOrWhiteSpace(kid))
                                    return keys.Where(k => k.KeyId == kid);

                                return keys;
                            }
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Para este cenário, mantenha UseJwks = true.");
                    }

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"[AUTH FAILED] {context.Exception}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("[TOKEN OK]");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"[CHALLENGE] {context.AuthenticateFailure?.Message}");
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
