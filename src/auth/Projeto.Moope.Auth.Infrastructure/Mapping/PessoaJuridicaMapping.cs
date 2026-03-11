using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Mapping
{
    public class PessoaJuridicaMapping : IEntityTypeConfiguration<PessoaJuridica>
    {
        public void Configure(EntityTypeBuilder<PessoaJuridica> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Cnpj)
                .IsRequired()
                .HasMaxLength(18);

            builder.Property(p => p.RazaoSocial)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.NomeFantasia)
                .HasMaxLength(255);

            builder.Property(p => p.InscricaoEstadual)
                .HasMaxLength(20);

            builder.Property(p => p.Created)
                .IsRequired();

            builder.Property(p => p.Updated)
                .IsRequired();

            builder.ToTable("PessoaJuridica");
        }
    }
}