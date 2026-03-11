using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Mapping
{
    public class PessoaFisicaMapping : IEntityTypeConfiguration<PessoaFisica>
    {
        public void Configure(EntityTypeBuilder<PessoaFisica> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Cpf)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(p => p.Created)
                .IsRequired();

            builder.Property(p => p.Updated)
                .IsRequired();

            builder.ToTable("PessoaFisica");
        }
    }
}