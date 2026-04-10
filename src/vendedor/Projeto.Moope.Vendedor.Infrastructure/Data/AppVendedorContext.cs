using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Infrastructure.Data
{
    public class AppVendedorContext : DbContext, IUnitOfWork
    {
        public AppVendedorContext(DbContextOptions<AppVendedorContext> options) : base(options) { }

        public DbSet<VendedorModel> Vendedores { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppVendedorContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
