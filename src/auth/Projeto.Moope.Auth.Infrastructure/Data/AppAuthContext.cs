using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Data
{
    public class AppAuthContext : DbContext
    {
        public AppAuthContext(DbContextOptions<AppAuthContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PessoaFisica> PessoasFisicas { get; set; }
        public DbSet<PessoaJuridica> PessoasJuridicas { get; set; }
        public DbSet<Papel> Papeis { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppAuthContext).Assembly);
        }
    }
}
