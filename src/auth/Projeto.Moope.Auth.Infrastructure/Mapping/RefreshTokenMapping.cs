using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Mapping
{
    public class RefreshTokenMapping : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.UsuarioId)
                .IsRequired();

            builder.Property(r => r.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.ExpiresAt)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.RevokedAt)
                .IsRequired(false);

            builder.Property(r => r.ReplacedByToken)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.HasIndex(r => r.Token);
            builder.HasIndex(r => r.UsuarioId);

            builder.ToTable("RefreshToken");
        }
    }
}
