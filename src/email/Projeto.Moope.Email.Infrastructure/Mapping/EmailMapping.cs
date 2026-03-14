using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Infrastructure.Mapping
{
    public class EmailMapping : IEntityTypeConfiguration<EmailModel>
    {
        public void Configure(EntityTypeBuilder<EmailModel> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Remetente)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.NomeRemetente)
                .HasMaxLength(255);

            builder.Property(e => e.Destinatario)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.NomeDestinatario)
                .HasMaxLength(255);

            builder.Property(e => e.Copia)
                .HasMaxLength(1000);

            builder.Property(e => e.CopiaOculta)
                .HasMaxLength(1000);

            builder.Property(e => e.Assunto)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.Corpo)
                .IsRequired();

            builder.Property(e => e.MensagemErro)
                .HasMaxLength(2000);

            builder.Property(e => e.MensagemSucesso)
                .HasMaxLength(500);

            builder.Property(e => e.Tipo)
                .HasMaxLength(100);

            builder.ToTable("Email");
        }
    }
}
