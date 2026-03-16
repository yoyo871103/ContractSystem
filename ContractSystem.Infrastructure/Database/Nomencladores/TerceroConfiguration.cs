using ContractSystem.Domain.Nomencladores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Nomencladores;

internal sealed class TerceroConfiguration : IEntityTypeConfiguration<Tercero>
{
    public void Configure(EntityTypeBuilder<Tercero> builder)
    {
        builder.ToTable("nom_Terceros");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Codigo)
            .HasMaxLength(64);

        builder.Property(e => e.Nombre)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.RazonSocial)
            .HasMaxLength(256);

        builder.Property(e => e.NifCif)
            .HasMaxLength(32);

        builder.Property(e => e.Direccion)
            .HasMaxLength(512);

        builder.Property(e => e.Telefono)
            .HasMaxLength(64);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.Tipo)
            .IsRequired();

        builder.Property(e => e.UbicacionExpediente)
            .HasMaxLength(128);

        builder.Property(e => e.DeletedAt);

        builder.HasIndex(e => e.NifCif);
        builder.HasIndex(e => e.Tipo);

        builder.HasMany(e => e.Contactos)
            .WithOne(c => c.Tercero)
            .HasForeignKey(c => c.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
