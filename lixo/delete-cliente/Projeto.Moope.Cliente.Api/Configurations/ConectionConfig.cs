using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Projeto.Moope.Cliente.Infrastructure.Data;

namespace Projeto.Moope.Cliente.Api.Configurations
{
    public static class ConectionConfig
    {
        public static IServiceCollection AddConectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

            services.AddDbContext<AppClienteContext>(options =>
                options.UseMySql(connectionString, serverVersion));

            return services;
        }
    }
}
