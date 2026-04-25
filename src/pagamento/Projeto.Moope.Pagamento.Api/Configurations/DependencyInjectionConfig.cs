using Microsoft.Extensions.Options;
using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Pagamento.Core.Configurations;
using Projeto.Moope.Pagamento.Core.Interfaces.Gateways;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;
using Projeto.Moope.Pagamento.Core.Services;
using Projeto.Moope.Pagamento.Infrastructure.Configurations;
using Projeto.Moope.Pagamento.Infrastructure.Repositories;
using Projeto.Moope.Pagamento.Infrastructure.Services;

namespace Projeto.Moope.Pagamento.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdempotencyKeyGeneratorOptions>(
                configuration.GetSection(IdempotencyKeyGeneratorOptions.SectionName));

            services.Configure<IntencaoPagamentoOptions>(
                configuration.GetSection(IntencaoPagamentoOptions.SectionName));

            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IUser, AspNetUser>();

            services.AddScoped<IIntencaoPagamentoRepository, IntencaoPagamentoRepository>();
            services.AddScoped<IIntencaoPagamentoService, IntencaoPagamentoService>();

            services.AddScoped<IPagamentoRepository, PagamentoRepository>();
            services.AddScoped<IPagamentoService, PagamentoService>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddScoped<IIdempotenciaService, IdempotenciaService>();
            services.AddSingleton<IGeradorHashRequisicao, GeradorHashRequisicaoSha256>();

            services.Configure<CelcoinPaymentsSettings>(configuration.GetSection(CelcoinPaymentsSettings.SectionName));

            // HttpClient tipado para o gateway Celcoin Payments (BaseUrl + token provider + client).
            services.AddHttpClient<ICelcoinTokenProvider, CelcoinTokenProvider>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CelcoinPaymentsSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            });

            services.AddHttpClient<ICelcoinPaymentGatewayClient, CelcoinPaymentGatewayClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CelcoinPaymentsSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            });

            return services;
        }
    }
}
