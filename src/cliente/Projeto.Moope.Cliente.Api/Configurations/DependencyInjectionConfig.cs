using Projeto.Moope.Api.Utils;
using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Services;
using Projeto.Moope.Cliente.Infrastructure.Repositories;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Cliente.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IUser, AspNetUser>();
            return services;
        }
    }
}
