using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Mapping
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.EnderecoId)
                .IsRequired(false);

            builder.Property(u => u.Created)
                .IsRequired();

            builder.Property(u => u.Updated)
                .IsRequired();

            // Relationships are handled via foreign keys, navigations are not mapped

            builder.ToTable("Usuario");
        }
    }
}