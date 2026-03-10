using Microsoft.EntityFrameworkCore;
using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Infrastructure.Data
{
    public class AppEnderecoContext : DbContext
    {
        public AppEnderecoContext(DbContextOptions<AppEnderecoContext> options) : base(options) { }

        public DbSet<EnderecoModel> Enderecos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EnderecoModel>().Property(e => e.Id).HasColumnType("char(36)");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppEnderecoContext).Assembly);
        }
    }
}
