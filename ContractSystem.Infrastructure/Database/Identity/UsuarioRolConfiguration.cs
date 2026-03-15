using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Identity;

internal sealed class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuarioRoles");

        builder.HasKey(e => new { e.UsuarioId, e.RolId });
    }
}
