using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Comodato.Core.Models;

namespace Projeto.Moope.Comodato.Infrastructure.Mapping
{
    public class ComodatoConviteMapping : IEntityTypeConfiguration<ComodatoConvite>
    {
        public void Configure(EntityTypeBuilder<ComodatoConvite> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(c => c.TokenHash)
                .IsUnique();

            builder.Property(c => c.CreatedByAdminId)
                .IsRequired();

            builder.Property(c => c.PlanoId)
                .IsRequired();

            builder.Property(c => c.Quantidade)
                .IsRequired();

            builder.Property(c => c.Valor)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(c => c.CriadoEm)
                .IsRequired();

            builder.Property(c => c.ExpiradoEm)
                .IsRequired();

            builder.Property(c => c.Status)
                .IsRequired();

            builder.Property(c => c.ClienteEmail)
                .HasMaxLength(255);

            builder.Property(c => c.ClienteDocumento)
                .HasMaxLength(20);

            builder.Property(c => c.Estado)
                .HasMaxLength(50);

            builder.HasOne(c => c.Comodato)
                .WithMany()
                .HasForeignKey(c => c.ComodatoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable("ComodatoConvite");
        }
    }
}
