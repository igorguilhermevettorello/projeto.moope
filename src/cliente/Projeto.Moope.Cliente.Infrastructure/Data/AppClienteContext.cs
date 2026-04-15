using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Infrastructure.Data
{
    public class AppClienteContext : DbContext, IUnitOfWork
    {
        public AppClienteContext(DbContextOptions<AppClienteContext> options) : base(options) { }

        public DbSet<ClienteModel> Clientes { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppClienteContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
