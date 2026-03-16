using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class ConfiguracionNumeracionConfiguration : IEntityTypeConfiguration<ConfiguracionNumeracion>
{
    public void Configure(EntityTypeBuilder<ConfiguracionNumeracion> builder)
    {
        builder.ToTable("ConfiguracionNumeracion");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Formato)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.DigitosPadding)
            .IsRequired();

        builder.Property(e => e.ContadorPorAnio)
            .IsRequired();

        builder.Property(e => e.Activa)
            .IsRequired();

        builder.Property(e => e.FechaCreacion);
    }
}
