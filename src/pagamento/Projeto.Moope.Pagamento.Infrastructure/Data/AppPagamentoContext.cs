using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Pagamento.Core.Models;

namespace Projeto.Moope.Pagamento.Infrastructure.Data
{
    public class AppPagamentoContext : DbContext, IUnitOfWork
    {
        public AppPagamentoContext(DbContextOptions<AppPagamentoContext> options) : base(options) { }

        public DbSet<PagamentoReferencia> PagamentoReferencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasColumnType("char(36)");
                }
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppPagamentoContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}

