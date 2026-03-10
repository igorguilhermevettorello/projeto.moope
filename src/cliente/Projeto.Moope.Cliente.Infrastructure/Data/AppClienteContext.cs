using Microsoft.EntityFrameworkCore;

namespace Projeto.Moope.Cliente.Infrastructure.Data
{
    public class AppClienteContext : DbContext
    {
        public AppClienteContext(DbContextOptions<AppClienteContext> options) : base(options) { }
    }
}
