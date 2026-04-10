using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Infrastructure.Mapping
{
    public class VendedorMapping : IEntityTypeConfiguration<VendedorModel>
    {
        public void Configure(EntityTypeBuilder<VendedorModel> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.PercentualComissao)
                .IsRequired()
                .HasColumnType("decimal(7,4)");

            builder.Property(v => v.ChavePix)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(v => v.CodigoCupom)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.Created)
                .IsRequired();

            builder.Property(v => v.Updated)
                .IsRequired();

            builder.HasOne(v => v.VendedorPai)
                .WithMany(v => v.VendedoresFilhos)
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Vendedor");
        }
    }
}
