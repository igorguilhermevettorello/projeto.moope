using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Pagamento.Infrastructure.Data;

namespace Projeto.Moope.Pagamento.Api.Configurations
{
    public static class ConectionConfig
    {
        public static IServiceCollection AddConectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppPagamentoContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0))
                )
            );

            return services;
        }
    }
}

