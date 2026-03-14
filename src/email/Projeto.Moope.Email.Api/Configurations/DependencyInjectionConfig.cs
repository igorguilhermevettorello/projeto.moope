using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Email.Core.Interfaces.Repositories;
using Projeto.Moope.Email.Core.Interfaces.Services;
using Projeto.Moope.Email.Core.Services;
using Projeto.Moope.Email.Infrastructure.Configurations;
using Projeto.Moope.Email.Infrastructure.Repositories;
using Projeto.Moope.Email.Infrastructure.Services;

namespace Projeto.Moope.Email.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmailService, EmailService>();

            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

            services.AddHttpClient<IEmailGateway, EmailGateway>(client =>
            {
                var emailSettings = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>();
                if (emailSettings != null && !string.IsNullOrEmpty(emailSettings.ApiUrl))
                {
                    client.BaseAddress = new Uri(emailSettings.ApiUrl.TrimEnd('/') + "/");
                }
            });
        }
    }
}
