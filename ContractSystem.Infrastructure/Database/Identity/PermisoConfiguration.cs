using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Identity;

internal sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permisos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(e => e.Nombre)
            .IsUnique();

        builder.Property(e => e.Descripcion)
            .HasMaxLength(256);

        builder.Property(e => e.Categoria)
            .HasMaxLength(64);

        builder.HasMany(e => e.RolPermisos)
            .WithOne(rp => rp.Permiso)
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
