using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Identity;

internal sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(e => e.Nombre)
            .IsUnique();

        builder.Property(e => e.Descripcion)
            .HasMaxLength(256);

        builder.HasMany(e => e.UsuarioRoles)
            .WithOne(ur => ur.Rol)
            .HasForeignKey(ur => ur.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RolPermisos)
            .WithOne(rp => rp.Rol)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
