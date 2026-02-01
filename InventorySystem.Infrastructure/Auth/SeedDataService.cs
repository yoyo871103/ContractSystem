using InventorySystem.Application.Auth;
using InventorySystem.Application.Database;
using InventorySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = InventorySystem.Infrastructure.Database.ApplicationDbContext;

namespace InventorySystem.Infrastructure.Auth;

internal sealed class SeedDataService : ISeedDataService
{
    private readonly IApplicationDbContextFactory _contextFactory;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Contraseña por defecto del usuario admin inicial. El usuario debe cambiarla en el primer login.
    /// </summary>
    private const string DefaultAdminPassword = "Admin123!";

    public SeedDataService(IApplicationDbContextFactory contextFactory, IPasswordHasher passwordHasher)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task EnsureSeedAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var anyUser = await db.Usuarios.AnyAsync(cancellationToken);
        if (anyUser)
            return;

        var (hash, salt) = _passwordHasher.HashPassword(DefaultAdminPassword);

        var rolAdmin = new Rol { Nombre = "Administrador" };
        db.Roles.Add(rolAdmin);
        await db.SaveChangesAsync(cancellationToken);

        var usuarioAdmin = new Usuario
        {
            NombreUsuario = "admin",
            NombreParaMostrar = "Administrador",
            HashContraseña = hash,
            Salt = salt,
            RequiereCambioContraseña = true,
            Activo = true,
            FechaCreacion = DateTimeOffset.UtcNow
        };
        db.Usuarios.Add(usuarioAdmin);
        await db.SaveChangesAsync(cancellationToken);

        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioAdmin.Id, RolId = rolAdmin.Id });
        await db.SaveChangesAsync(cancellationToken);
    }
}
