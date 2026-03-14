using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using Projeto.Moope.Comodato.Core.Services;
using Projeto.Moope.Comodato.Infrastructure.Repositories;

namespace Projeto.Moope.Comodato.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IComodatoRepository, ComodatoRepository>();
            services.AddScoped<IComodatoService, ComodatoService>();
        }
    }
}
