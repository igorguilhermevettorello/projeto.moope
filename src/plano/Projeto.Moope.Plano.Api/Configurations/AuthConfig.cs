using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Projeto.Moope.Plano.Api.Utils;
using Projeto.Moope.Plano.Api.Utils;
using System.Net.Http;
using System.Text;

namespace Projeto.Moope.Plano.Api.Configurations
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
                        // Authority e emissão do token continuam em HTTPS. O backchannel é só o HttpClient interno
                        // que baixa /.well-known e JWKS; entre dois Kestrel em localhost o certificado de dev costuma falhar na validação TLS.
                        options.BackchannelHttpHandler = new LoggingHandler
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Breakpoint aqui: token chegou? header Authorization está presente?
                        var token = context.Token; // null se não veio no header
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith("Bearer "))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Breakpoint aqui: autenticação falhou
                        // context.Exception diz EXATAMENTE o que aconteceu
                        var erro = context.Exception.Message;         // mensagem de erro
                        var tipo = context.Exception.GetType().Name;  // tipo da exceção
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Breakpoint aqui: token foi validado com sucesso
                        // Se chegar aqui, autenticação passou
                        var user = context.Principal?.Identity?.Name;
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Breakpoint aqui: 401 está sendo enviado
                        // context.AuthenticateFailure tem o motivo
                        var motivo = context.AuthenticateFailure?.Message;
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
