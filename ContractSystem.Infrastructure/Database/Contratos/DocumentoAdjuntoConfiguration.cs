using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

public class DocumentoAdjuntoConfiguration : IEntityTypeConfiguration<DocumentoAdjunto>
{
    public void Configure(EntityTypeBuilder<DocumentoAdjunto> builder)
    {
        builder.ToTable("DocumentosAdjuntos");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NombreArchivo).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Extension).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Objetivo).HasMaxLength(1000).IsRequired();
        builder.Property(d => d.TamanioBytes).IsRequired();
        builder.Property(d => d.FechaCarga).IsRequired();

        builder.HasOne(d => d.Contrato)
            .WithMany(c => c.DocumentosAdjuntos)
            .HasForeignKey(d => d.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
