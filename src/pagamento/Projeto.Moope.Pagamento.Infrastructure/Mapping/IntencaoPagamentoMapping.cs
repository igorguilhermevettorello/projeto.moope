using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pagamento.Core.Enums;
using IntencaoModel = Projeto.Moope.Pagamento.Core.Models.IntencaoPagamento;

namespace Projeto.Moope.Pagamento.Infrastructure.Mapping
{
    public class IntencaoPagamentoMapping : IEntityTypeConfiguration<IntencaoModel>
    {
        public void Configure(EntityTypeBuilder<IntencaoModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Valor)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Moeda)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<StatusIntencaoPagamento>(v))
                .HasMaxLength(20);

            builder.Property(x => x.MetodoPagamento)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<MetodoPagamento>(v))
                .HasMaxLength(20);

            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("IX_IntencaoPagamento_ExpiresAt");

            builder.ToTable("IntencaoPagamento");
        }
    }
}
