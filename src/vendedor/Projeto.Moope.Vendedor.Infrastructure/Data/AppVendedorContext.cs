using Microsoft.EntityFrameworkCore;

namespace Projeto.Moope.Vendedor.Infrastructure.Data
{
    public class AppVendedorContext : DbContext
    {
        public AppVendedorContext(DbContextOptions<AppVendedorContext> options) : base(options) { }
    }
}
