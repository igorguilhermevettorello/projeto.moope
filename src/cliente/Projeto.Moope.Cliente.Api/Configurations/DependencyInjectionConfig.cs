using Projeto.Moope.Cliente.Core.Interfaces;
using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Services;
using Projeto.Moope.Cliente.Infrastructure.Data;
using Projeto.Moope.Cliente.Infrastructure.Handlers;
using Projeto.Moope.Cliente.Infrastructure.Repositories;
using Projeto.Moope.Cliente.Infrastructure.Services;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Cliente.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IClienteUnitOfWork>(sp => sp.GetRequiredService<AppClienteContext>());
            services.AddScoped<IClienteRepository, ClienteRepository>();

            services.AddScoped<IIdentityUserService, IdentityUserService>();
            services.AddScoped<IClienteService, ClienteService>();

            services.AddAutoMapper(typeof(AutomapperClienteProfile));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CriarClienteCommandHandler).Assembly);
            });
        }
    }
}
