using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Infrastructure.Data
{
    public class AppEmailContext : DbContext, IUnitOfWork
    {
        public AppEmailContext(DbContextOptions<AppEmailContext> options) : base(options) { }

        public DbSet<EmailModel> Emails { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppEmailContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
