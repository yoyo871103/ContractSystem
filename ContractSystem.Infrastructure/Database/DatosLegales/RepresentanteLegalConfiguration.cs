using ContractSystem.Domain.DatosLegales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.DatosLegales;

internal sealed class RepresentanteLegalConfiguration : IEntityTypeConfiguration<RepresentanteLegal>
{
    public void Configure(EntityTypeBuilder<RepresentanteLegal> builder)
    {
        builder.ToTable("RepresentantesLegales");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Cargo).HasMaxLength(128).IsRequired();
        builder.Property(e => e.CarneIdentidad).HasMaxLength(32);
        builder.Property(e => e.DocumentoNombramiento).HasMaxLength(512);

        builder.HasOne(e => e.BusinessInfo)
            .WithMany(b => b.Representantes)
            .HasForeignKey(e => e.BusinessInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Tercero)
            .WithMany(t => t.Representantes)
            .HasForeignKey(e => e.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
