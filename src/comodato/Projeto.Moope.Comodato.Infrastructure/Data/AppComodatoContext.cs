using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Infrastructure.Data
{
    public class AppComodatoContext : DbContext, IUnitOfWork
    {
        public AppComodatoContext(DbContextOptions<AppComodatoContext> options) : base(options) { }

        public DbSet<ComodatoModel> Comodatos { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppComodatoContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
