using Projeto.Moope.Api.Utils;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using Projeto.Moope.Comodato.Core.Services;
using Projeto.Moope.Comodato.Infrastructure.Repositories;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Comodato.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IComodatoRepository, ComodatoRepository>();
            services.AddScoped<IComodatoConviteRepository, ComodatoConviteRepository>();
            services.AddScoped<IComodatoService, ComodatoService>();
            services.AddScoped<IComodatoConviteService, ComodatoConviteService>();
            services.AddScoped<IUser, AspNetUser>();
            return services;
        }
    }
}
