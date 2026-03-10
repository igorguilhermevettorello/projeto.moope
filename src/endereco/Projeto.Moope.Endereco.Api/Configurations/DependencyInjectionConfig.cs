using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Endereco.Core.Interfaces.Repositories;
using Projeto.Moope.Endereco.Core.Interfaces.Services;
using Projeto.Moope.Endereco.Core.Services;
using Projeto.Moope.Endereco.Infrastructure.Data;
using Projeto.Moope.Endereco.Infrastructure.Repositories;

namespace Projeto.Moope.Endereco.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Notificações
            services.AddScoped<INotificador, Notificador>();

            // Repositories
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            
            // Services
            services.AddScoped<IEnderecoService, EnderecoService>();
        }
    }
}
