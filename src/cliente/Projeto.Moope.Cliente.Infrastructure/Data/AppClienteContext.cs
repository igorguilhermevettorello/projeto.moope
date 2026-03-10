using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Cliente.Core.Models;

namespace Projeto.Moope.Cliente.Infrastructure.Data
{
    public class AppClienteContext : DbContext
    {
        public AppClienteContext(DbContextOptions<AppClienteContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
    }
}
