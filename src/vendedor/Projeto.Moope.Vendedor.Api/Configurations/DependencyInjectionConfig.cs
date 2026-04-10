using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Vendedor.Core.Interfaces.Repositories;
using Projeto.Moope.Vendedor.Core.Interfaces.Services;
using Projeto.Moope.Vendedor.Core.Services;
using Projeto.Moope.Vendedor.Infrastructure.Repositories;

namespace Projeto.Moope.Vendedor.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IVendedorRepository, VendedorRepository>();
            services.AddScoped<IVendedorService, VendedorService>();
        }
    }
}
