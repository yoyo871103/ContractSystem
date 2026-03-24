using ContractSystem.Domain.Licensing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Licensing;

internal sealed class LicenciaInfoConfiguration : IEntityTypeConfiguration<LicenciaInfo>
{
    public void Configure(EntityTypeBuilder<LicenciaInfo> builder)
    {
        builder.ToTable("Licencias");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Clave)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(e => e.FechaActivacion)
            .IsRequired();

        builder.Property(e => e.FechaExpiracion)
            .IsRequired();
    }
}
