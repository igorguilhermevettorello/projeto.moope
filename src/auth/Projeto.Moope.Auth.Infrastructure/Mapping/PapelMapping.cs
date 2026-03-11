using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Mapping
{
    public class PapelMapping : IEntityTypeConfiguration<Papel>
    {
        public void Configure(EntityTypeBuilder<Papel> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.TipoUsuario)
                .IsRequired();

            builder.Property(p => p.Created)
                .IsRequired();

            builder.Property(p => p.Updated)
                .IsRequired();

            builder.HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Papel");
        }
    }
}