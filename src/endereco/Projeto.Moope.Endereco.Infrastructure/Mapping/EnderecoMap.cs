using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Infrastructure.Mapping
{
    public class EnderecoMap : IEntityTypeConfiguration<EnderecoModel>
    {
        public void Configure(EntityTypeBuilder<EnderecoModel> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Cep)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.Logradouro)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Numero)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.Complemento)
                .HasMaxLength(255);

            builder.Property(e => e.Bairro)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Cidade)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.Created)
                .IsRequired();

            builder.Property(e => e.Updated)
                .IsRequired();
        }
    }
}