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

        // Restrict para no chocar con la cascada Contratos -> DocumentosAdjuntos en SQL Server.
        // El borrado de adjuntos asociados a una factura se hace explícitamente al eliminar la factura.
        builder.HasOne(d => d.Factura)
            .WithMany(f => f.DocumentosAdjuntos)
            .HasForeignKey(d => d.FacturaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(d => d.FacturaId);

        // Restrict por el mismo motivo (Contratos -> Anexos ya cascadea). El borrado de los
        // adjuntos de un anexo se hace explícitamente al eliminar el anexo.
        builder.HasOne(d => d.Anexo)
            .WithMany(a => a.DocumentosAdjuntos)
            .HasForeignKey(d => d.AnexoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(d => d.AnexoId);
    }
}
