using ContractSystem.Domain.Nomencladores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Nomencladores;

internal sealed class PlantillaDocumentoConfiguration : IEntityTypeConfiguration<PlantillaDocumento>
{
    public void Configure(EntityTypeBuilder<PlantillaDocumento> builder)
    {
        builder.ToTable("nom_PlantillasDocumento");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.TipoDocumento)
            .IsRequired();

        builder.Property(e => e.Rol)
            .IsRequired();

        builder.Property(e => e.Archivo)
            .IsRequired();

        builder.Property(e => e.NombreArchivo)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.FechaCreacion);

        builder.Property(e => e.RevisadoPorLegal);

        builder.HasIndex(e => e.TipoDocumento);
        builder.HasIndex(e => e.Rol);
    }
}
