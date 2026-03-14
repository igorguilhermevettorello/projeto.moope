using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Infrastructure.Mapping
{
    public class ComodatoMapping : IEntityTypeConfiguration<ComodatoModel>
    {
        public void Configure(EntityTypeBuilder<ComodatoModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClienteId)
                .IsRequired();

            builder.Property(c => c.ProdutoNome)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(c => c.Valor)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(c => c.CriadoEm)
                .IsRequired();

            builder.Property(c => c.Status)
                .IsRequired();

            builder.ToTable("Comodato");
        }
    }
}
