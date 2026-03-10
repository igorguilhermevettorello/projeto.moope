using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Vendedor.Infrastructure.Data;

namespace Projeto.Moope.Vendedor.Api.Configurations
{
    public static class ConectionConfig
    {
        public static IServiceCollection AddConectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppVendedorContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0))
                )
            );

            return services;
        }
    }
}
