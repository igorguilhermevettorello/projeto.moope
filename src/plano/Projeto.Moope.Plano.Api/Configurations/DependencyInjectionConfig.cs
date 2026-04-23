using AutoMapper;
using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Plano.Api.Mappings;
using Projeto.Moope.Plano.Core.Interfaces.Repositories;
using Projeto.Moope.Plano.Core.Interfaces.Services;
using Projeto.Moope.Plano.Core.Services;
using Projeto.Moope.Plano.Infrastructure.Repositories;

namespace Projeto.Moope.Plano.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(PlanoMappingProfile).Assembly);

            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IPlanoRepository, PlanoRepository>();
            services.AddScoped<IPlanoService, PlanoService>();
            services.AddScoped<IUser, AspNetUser>();
            return services;
        }
    }
}
