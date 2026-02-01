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

        if (!anyUser)
        {
            await SeedInitialAdminAsync(db, cancellationToken);
            return;
        }

        await EnsurePermisosAsync(db, cancellationToken);
    }

    private async Task SeedInitialAdminAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        var (hash, salt) = _passwordHasher.HashPassword(DefaultAdminPassword);

        var rolAdmin = new Rol { Nombre = "Administrador" };
        var rolGestorUsuarios = new Rol { Nombre = "Gestor de usuarios" };
        db.Roles.Add(rolAdmin);
        db.Roles.Add(rolGestorUsuarios);
        await db.SaveChangesAsync(cancellationToken);

        var permisoGestionarUsuarios = new Permiso
        {
            Nombre = "GestionarUsuarios",
            Descripcion = "Gestionar usuarios (ver pestaña y realizar CRUD)"
        };
        db.Permisos.Add(permisoGestionarUsuarios);
        await db.SaveChangesAsync(cancellationToken);

        db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisoGestionarUsuarios.Id });
        db.RolPermisos.Add(new RolPermiso { RolId = rolGestorUsuarios.Id, PermisoId = permisoGestionarUsuarios.Id });
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

    /// <summary>
    /// Asegura que existan el permiso GestionarUsuarios y el rol Gestor de usuarios (para BDs ya existentes tras la migración).
    /// </summary>
    private static async Task EnsurePermisosAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        var anyPermiso = await db.Permisos.AnyAsync(cancellationToken);
        if (anyPermiso)
            return;

        var permiso = new Permiso
        {
            Nombre = "GestionarUsuarios",
            Descripcion = "Gestionar usuarios (ver pestaña y realizar CRUD)"
        };
        db.Permisos.Add(permiso);
        await db.SaveChangesAsync(cancellationToken);

        var rolAdmin = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador", cancellationToken);
        if (rolAdmin != null)
        {
            db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permiso.Id });
            await db.SaveChangesAsync(cancellationToken);
        }

        var rolGestor = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Gestor de usuarios", cancellationToken);
        if (rolGestor == null)
        {
            rolGestor = new Rol { Nombre = "Gestor de usuarios" };
            db.Roles.Add(rolGestor);
            await db.SaveChangesAsync(cancellationToken);
        }
        if (!await db.RolPermisos.AnyAsync(rp => rp.RolId == rolGestor.Id && rp.PermisoId == permiso.Id, cancellationToken))
        {
            db.RolPermisos.Add(new RolPermiso { RolId = rolGestor.Id, PermisoId = permiso.Id });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
