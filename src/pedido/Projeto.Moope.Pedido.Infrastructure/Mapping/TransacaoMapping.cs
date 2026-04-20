using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Infrastructure.Mapping
{
    public class TransacaoMapping : IEntityTypeConfiguration<TransacaoModel>
    {
        public void Configure(EntityTypeBuilder<TransacaoModel> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.PedidoId)
                .IsRequired();

            builder.Property(t => t.Valor)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(t => t.DataPagamento)
                .IsRequired();

            builder.Property(t => t.MetodoPagamento)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Status)
                .HasMaxLength(50);

            builder.Property(t => t.StatusDescricao)
                .HasMaxLength(255);

            builder.Property(t => t.Created)
                .IsRequired();

            builder.Property(t => t.Updated)
                .IsRequired();

            builder.ToTable("Transacao");
        }
    }
}

