using InventorySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Database.Identity;

internal sealed class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("RolPermisos");

        builder.HasKey(e => new { e.RolId, e.PermisoId });

        builder.HasOne(e => e.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(e => e.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(e => e.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.PermisoId);
    }
}
