using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Projeto.Moope.Pedido.Infrastructure.Data
{
    public class AppPedidoContextFactory : IDesignTimeDbContextFactory<AppPedidoContext>
    {
        public AppPedidoContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                                   ?? "Server=localhost;Port=3306;Database=moope_pedido;Uid=root;Pwd=;";

            var optionsBuilder = new DbContextOptionsBuilder<AppPedidoContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));

            return new AppPedidoContext(optionsBuilder.Options);
        }
    }
}

