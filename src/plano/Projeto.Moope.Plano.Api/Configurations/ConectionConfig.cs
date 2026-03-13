using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Plano.Infrastructure.Data;

namespace Projeto.Moope.Plano.Api.Configurations
{
    public static class ConectionConfig
    {
        public static IServiceCollection AddConectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppPlanoContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0))
                )
            );

            return services;
        }
    }
}
