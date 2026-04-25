using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;
using DescontoModel = Projeto.Moope.Pedido.Core.Models.Desconto;
using IdempotenciaModel = Projeto.Moope.Pedido.Core.Models.IdempotenciaPedido;

namespace Projeto.Moope.Pedido.Infrastructure.Data
{
    public class AppPedidoContext : DbContext, IUnitOfWork
    {
        public AppPedidoContext(DbContextOptions<AppPedidoContext> options) : base(options) { }

        public DbSet<PedidoModel> Pedidos { get; set; }
        public DbSet<TransacaoModel> Transacoes { get; set; }
        public DbSet<DescontoModel> Descontos { get; set; }
        public DbSet<IdempotenciaModel> Idempotencias { get; set; }

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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppPedidoContext).Assembly);
        }

        public async Task<bool> Commit()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}

