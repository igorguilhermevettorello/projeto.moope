using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Email.Infrastructure.Data;

namespace Projeto.Moope.Email.Api.Configurations
{
    public static class ConectionConfig
    {
        public static IServiceCollection AddConectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppEmailContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0))
                )
            );

            return services;
        }
    }
}
