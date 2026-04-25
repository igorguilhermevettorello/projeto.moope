using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using PagamentoModel = Projeto.Moope.Pagamento.Core.Models.Pagamento;
using IdempotenciaModel = Projeto.Moope.Pagamento.Core.Models.IdempotenciaPagamento;
using IntencaoModel = Projeto.Moope.Pagamento.Core.Models.IntencaoPagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Data
{
    public class AppPagamentoContext : DbContext, IUnitOfWork
    {
        public AppPagamentoContext(DbContextOptions<AppPagamentoContext> options) : base(options) { }

        public DbSet<PagamentoModel> Pagamentos { get; set; }
        public DbSet<IdempotenciaModel> Idempotencias { get; set; }
        public DbSet<IntencaoModel> IntencoesPagamento { get; set; }

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
