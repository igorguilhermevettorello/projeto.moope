using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Plano.Core.Interfaces.Repositories;
using Projeto.Moope.Plano.Core.Interfaces.Services;
using Projeto.Moope.Plano.Core.Services;
using Projeto.Moope.Plano.Infrastructure.Repositories;

namespace Projeto.Moope.Plano.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IPlanoRepository, PlanoRepository>();
            services.AddScoped<IPlanoService, PlanoService>();
        }
    }
}
