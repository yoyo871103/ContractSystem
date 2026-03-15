using ContractSystem.Domain.Nomencladores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Nomencladores;

internal sealed class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("nom_UnidadesMedida");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NombreCorto)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasMaxLength(256);

        builder.Property(e => e.DeletedAt);

        builder.HasIndex(e => e.NombreCorto)
            .IsUnique();
    }
}
