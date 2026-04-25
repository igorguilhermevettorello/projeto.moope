using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Pedido.Api.Mappings;
using Projeto.Moope.Pedido.Core.Interfaces.Gateways;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using Projeto.Moope.Pedido.Core.Options;
using Projeto.Moope.Pedido.Core.Queries.Plano.ObterPlanoPorId;
using Projeto.Moope.Pedido.Core.Services;
using Projeto.Moope.Pedido.Infrastructure.Gateways;
using Projeto.Moope.Pedido.Infrastructure.Handlers;
using Projeto.Moope.Pedido.Infrastructure.Repositories;

namespace Projeto.Moope.Pedido.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(PedidoMappingProfile).Assembly);

            services.Configure<PlanoApiOptions>(configuration.GetSection(PlanoApiOptions.SectionName));

            services.AddTransient<AuthorizationForwardingHandler>();
            
            services.AddHttpClient<IPlanoReadGateway, PlanoHttpReadGateway>((serviceProvider, client) =>
            {
                var planoApiOptions = serviceProvider.GetRequiredService<IOptions<PlanoApiOptions>>().Value;
                if (string.IsNullOrWhiteSpace(planoApiOptions.BaseUrl))
                    throw new InvalidOperationException("Configure PlanoApi:BaseUrl (URL base da API de planos).");

                client.BaseAddress = new Uri(planoApiOptions.BaseUrl.TrimEnd('/') + "/");
            }).AddHttpMessageHandler<AuthorizationForwardingHandler>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ObterPlanoPorIdQuery).Assembly));

            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IDescontoRepository, DescontoRepository>();
            services.AddScoped<IDescontoService, DescontoService>();
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddScoped<IIdempotenciaService, IdempotenciaService>();
            services.AddSingleton<IGeradorHashRequisicao, GeradorHashRequisicaoSha256>();
            services.AddScoped<IUser, AspNetUser>();
            return services;
        }
    }
}

