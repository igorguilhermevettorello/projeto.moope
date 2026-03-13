using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Infrastructure.Mapping
{
    public class PlanoMapping : IEntityTypeConfiguration<PlanoModel>
    {
        public void Configure(EntityTypeBuilder<PlanoModel> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Valor)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.TaxaAdesao)
                .HasColumnType("decimal(15,2)");

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Plataforma)
                .IsRequired();

            builder.ToTable("Plano");
        }
    }
}
