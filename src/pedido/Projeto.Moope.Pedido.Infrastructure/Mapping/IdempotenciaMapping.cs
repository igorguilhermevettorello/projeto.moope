using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Pedido.Core.Enums;
using IdempotenciaModel = Projeto.Moope.Pedido.Core.Models.IdempotenciaPedido;

namespace Projeto.Moope.Pedido.Infrastructure.Mapping
{
    public class IdempotenciaMapping : IEntityTypeConfiguration<IdempotenciaModel>
    {
        public void Configure(EntityTypeBuilder<IdempotenciaModel> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.IdempotencyKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.Scope)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.RequestHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<StatusIdempotencia>(v))
                .HasMaxLength(20);

            builder.Property(i => i.ResponseStatusCode);

            builder.Property(i => i.ResponseBody)
                .HasColumnType("text");

            builder.Property(i => i.ResourceId)
                .HasMaxLength(100);

            builder.Property(i => i.ResourceType)
                .HasMaxLength(50);

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.Property(i => i.UpdatedAt)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(i => new { i.IdempotencyKey, i.Scope })
                .IsUnique()
                .HasDatabaseName("UQ_Idempotency");

            builder.ToTable("IdempotenciaPedido");
        }
    }
}

