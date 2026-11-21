using ContractSystem.Domain.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Business;

/// <summary>
/// Solo fija las longitudes de los datos legales; el resto de columnas sigue las convenciones
/// con las que se creó la tabla (migración AddBusinessInfo).
/// </summary>
internal sealed class BusinessInfoConfiguration : IEntityTypeConfiguration<BusinessInfo>
{
    public void Configure(EntityTypeBuilder<BusinessInfo> builder)
    {
        builder.Property(e => e.CodigoReeup).HasMaxLength(32);
        builder.Property(e => e.RegistroMercantil).HasMaxLength(256);
        builder.Property(e => e.DocumentoConstitucion).HasMaxLength(512);
        builder.Property(e => e.PlantillaComparecencia).HasMaxLength(4000);
        builder.Property(e => e.PlantillaFirma).HasMaxLength(2000);
    }
}
