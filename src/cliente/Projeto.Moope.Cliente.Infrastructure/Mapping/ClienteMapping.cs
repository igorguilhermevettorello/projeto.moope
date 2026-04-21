using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Infrastructure.Mapping
{
    public class ClienteMapping : IEntityTypeConfiguration<ClienteModel>
    {
        public void Configure(EntityTypeBuilder<ClienteModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Telefone)
                .HasMaxLength(255);

            builder.Property(c => c.TelefoneEmergencia)
                .HasMaxLength(255);

            builder.Property(c => c.VendedorId)
                .IsRequired(false);

            builder.Property(u => u.EnderecoId)
                .IsRequired(false);

            builder.Property(c => c.Created)
                .IsRequired();

            builder.Property(c => c.Updated)
                .IsRequired();

            builder.Property(c => c.GalaxPayId)
                .IsRequired(false);

            builder.ToTable("Cliente");
        }
    }
}
