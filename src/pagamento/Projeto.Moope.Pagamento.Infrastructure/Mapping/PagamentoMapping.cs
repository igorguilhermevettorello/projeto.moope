using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PagamentoModel = Projeto.Moope.Pagamento.Core.Models.Pagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Mapping
{
    public class PagamentoMapping : IEntityTypeConfiguration<PagamentoModel>
    {
        public void Configure(EntityTypeBuilder<PagamentoModel> builder)
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

            builder.HasIndex(x => x.ClienteId);
            builder.HasIndex(x => x.GatewayCustomerId);

            builder.ToTable("Pagamento");
        }
    }
}

