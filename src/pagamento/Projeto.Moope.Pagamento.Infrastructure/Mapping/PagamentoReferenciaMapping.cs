using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Pagamento.Core.Models;

namespace Projeto.Moope.Pagamento.Infrastructure.Mapping
{
    public class PagamentoReferenciaMapping : IEntityTypeConfiguration<PagamentoReferencia>
    {
        public void Configure(EntityTypeBuilder<PagamentoReferencia> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClienteId)
                .IsRequired();

            builder.Property(x => x.GatewayCustomerId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.GatewayPlanId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.GatewaySubscriptionId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.Created).IsRequired();
            builder.Property(x => x.Updated).IsRequired();

            builder.HasIndex(x => x.ClienteId).IsUnique();
            builder.HasIndex(x => x.GatewayCustomerId).IsUnique();

            builder.ToTable("PagamentoReferencia");
        }
    }
}

