using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;

namespace Projeto.Moope.Pedido.Infrastructure.Mapping
{
    public class PedidoMapping : IEntityTypeConfiguration<PedidoModel>
    {
        public void Configure(EntityTypeBuilder<PedidoModel> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ClienteId)
                .IsRequired();

            builder.Property(p => p.PlanoId)
                .IsRequired();

            builder.Property(p => p.Quantidade)
                .IsRequired();

            builder.Property(p => p.PlanoValor)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.PlanoDescricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.PlanoCodigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.PlanoTaxaAdesao)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.PlanoPercentualDesconto)
                .IsRequired()
                .HasColumnType("decimal(7,4)");

            builder.Property(p => p.PlanoValorTotal)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.PlanoTaxaAdesaoTotal)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.Status)
                .HasMaxLength(50);

            builder.Property(p => p.StatusDescricao)
                .HasMaxLength(255);

            builder.Property(p => p.Created)
                .IsRequired();

            builder.Property(p => p.Updated)
                .IsRequired();

            builder.Property(p => p.TipoPessoa)
                .IsRequired();

            builder.Property(p => p.Estado)
                .HasMaxLength(50);

            builder.Property(p => p.TipoPlataforma)
                .HasMaxLength(50);

            builder.HasMany(p => p.Transacoes)
                .WithOne(t => t.Pedido)
                .HasForeignKey(t => t.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Desconto)
                .WithOne(d => d.Pedido)
                .HasForeignKey<Projeto.Moope.Pedido.Core.Models.Desconto>(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Pedido");
        }
    }
}

