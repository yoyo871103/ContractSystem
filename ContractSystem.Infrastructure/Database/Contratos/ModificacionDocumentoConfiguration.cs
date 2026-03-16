using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class ModificacionDocumentoConfiguration : IEntityTypeConfiguration<ModificacionDocumento>
{
    public void Configure(EntityTypeBuilder<ModificacionDocumento> builder)
    {
        builder.ToTable("ModificacionesDocumento");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(e => e.FechaRegistro);

        // Índice único para evitar duplicados de relación
        builder.HasIndex(e => new { e.DocumentoOrigenId, e.DocumentoDestinoId })
            .IsUnique();

        // Relación: Documento Origen "modifica a" → sus ModificaA
        builder.HasOne(e => e.DocumentoOrigen)
            .WithMany(c => c.ModificaA)
            .HasForeignKey(e => e.DocumentoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación: Documento Destino "modificado por" → sus ModificadoPor
        builder.HasOne(e => e.DocumentoDestino)
            .WithMany(c => c.ModificadoPor)
            .HasForeignKey(e => e.DocumentoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
