using InventorySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Database.Identity;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NombreUsuario)
            .HasMaxLength(128)
            .IsRequired();
        builder.HasIndex(e => e.NombreUsuario)
            .IsUnique();

        builder.Property(e => e.NombreParaMostrar)
            .HasMaxLength(256);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.FotoPerfil)
            .HasMaxLength(1024 * 512); // ~512 KB máx.

        builder.Property(e => e.DeletedAt);

        // El filtro global de soft delete se aplica en ApplicationDbContext.ApplySoftDeleteQueryFilters.

        builder.Property(e => e.HashContraseña)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.Salt)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.RequiereCambioContraseña)
            .HasDefaultValue(false);

        builder.Property(e => e.Activo)
            .HasDefaultValue(true);

        builder.HasMany(e => e.UsuarioRoles)
            .WithOne(ur => ur.Usuario)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
