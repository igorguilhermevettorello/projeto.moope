using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using Projeto.Moope.Pedido.Core.Services;
using Projeto.Moope.Pedido.Infrastructure.Repositories;

namespace Projeto.Moope.Pedido.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IUser, AspNetUser>();
        }
    }
}

