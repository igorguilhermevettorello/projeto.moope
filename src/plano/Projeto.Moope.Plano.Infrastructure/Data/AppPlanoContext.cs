using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Infrastructure.Data
{
    public class AppPlanoContext : DbContext, IUnitOfWork
    {
        public AppPlanoContext(DbContextOptions<AppPlanoContext> options) : base(options) { }

        public DbSet<PlanoModel> Planos { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppPlanoContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
