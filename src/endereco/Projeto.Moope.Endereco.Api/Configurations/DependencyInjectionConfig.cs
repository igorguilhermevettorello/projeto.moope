using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Endereco.Core.Interfaces.Repositories;
using Projeto.Moope.Endereco.Core.Interfaces.Services;
using Projeto.Moope.Endereco.Core.Services;
using Projeto.Moope.Endereco.Infrastructure.Repositories;

namespace Projeto.Moope.Endereco.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IEnderecoService, EnderecoService>();
            services.AddScoped<IUser, AspNetUser>();
        }
    }
}
