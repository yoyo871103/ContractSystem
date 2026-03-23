using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Identity;

internal sealed class UsuarioPermisoConfiguration : IEntityTypeConfiguration<UsuarioPermiso>
{
    public void Configure(EntityTypeBuilder<UsuarioPermiso> builder)
    {
        builder.ToTable("UsuarioPermisos");

        builder.HasKey(e => new { e.UsuarioId, e.PermisoId });

        builder.HasOne(e => e.Usuario)
            .WithMany(u => u.UsuarioPermisos)
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permiso)
            .WithMany(p => p.UsuarioPermisos)
            .HasForeignKey(e => e.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
