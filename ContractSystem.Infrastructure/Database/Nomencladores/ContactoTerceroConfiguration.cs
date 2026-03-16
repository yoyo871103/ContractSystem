using ContractSystem.Domain.Nomencladores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Nomencladores;

internal sealed class ContactoTerceroConfiguration : IEntityTypeConfiguration<ContactoTercero>
{
    public void Configure(EntityTypeBuilder<ContactoTercero> builder)
    {
        builder.ToTable("nom_ContactosTercero");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Cargo)
            .HasMaxLength(128);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.Telefono)
            .HasMaxLength(64);
    }
}
