using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DescontoModel = Projeto.Moope.Pedido.Core.Models.Desconto;

namespace Projeto.Moope.Pedido.Infrastructure.Mapping
{
    public class DescontoMapping : IEntityTypeConfiguration<DescontoModel>
    {
        public void Configure(EntityTypeBuilder<DescontoModel> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.PedidoId)
                .IsRequired();

            builder.Property(d => d.ValorPercentual)
                .IsRequired()
                .HasColumnType("decimal(7,4)");

            builder.Property(d => d.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.CodigoDesconto)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.ValorDesconto)
                .HasColumnType("decimal(15,2)");

            builder.Property(d => d.Ativo)
                .IsRequired();

            builder.ToTable("Desconto");
        }
    }
}

