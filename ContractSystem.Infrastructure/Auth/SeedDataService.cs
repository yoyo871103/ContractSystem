using ContractSystem.Application.Auth;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Auth;

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

        await EnsureRolAdministradorSistemaAsync(db, cancellationToken);
    }

    private async Task SeedInitialAdminAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        var (hash, salt) = _passwordHasher.HashPassword(DefaultAdminPassword);

        var rolAdmin = new Rol { Nombre = DefaultUsers.RolAdministradorSistema };
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
    /// Asegura que exista el rol "Administrador de sistema", que tenga TODOS los permisos
    /// de la tabla Permisos, y que existan los permisos y roles base.
    /// Renombra el antiguo "Administrador" si existe.
    /// </summary>
    private static async Task EnsureRolAdministradorSistemaAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        // --- Permisos base ---
        var permisoGU = await db.Permisos.FirstOrDefaultAsync(p => p.Nombre == "GestionarUsuarios", cancellationToken);
        if (permisoGU is null)
        {
            permisoGU = new Permiso
            {
                Nombre = "GestionarUsuarios",
                Descripcion = "Gestionar usuarios (ver pestaña y realizar CRUD)"
            };
            db.Permisos.Add(permisoGU);
            await db.SaveChangesAsync(cancellationToken);
        }

        // --- Rol Administrador de sistema (renombra si viene del antiguo "Administrador") ---
        var rolAdmin = await db.Roles.FirstOrDefaultAsync(
            r => r.Nombre == DefaultUsers.RolAdministradorSistema || r.Nombre == "Administrador", cancellationToken);
        if (rolAdmin is null)
        {
            rolAdmin = new Rol { Nombre = DefaultUsers.RolAdministradorSistema };
            db.Roles.Add(rolAdmin);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (rolAdmin.Nombre != DefaultUsers.RolAdministradorSistema)
        {
            rolAdmin.Nombre = DefaultUsers.RolAdministradorSistema;
            await db.SaveChangesAsync(cancellationToken);
        }

        // Asignar TODOS los permisos al rol Administrador de sistema
        var todosLosPermisoIds = await db.Permisos.Select(p => p.Id).ToListAsync(cancellationToken);
        var permisosYaAsignados = await db.RolPermisos
            .Where(rp => rp.RolId == rolAdmin.Id)
            .Select(rp => rp.PermisoId)
            .ToListAsync(cancellationToken);
        var faltantes = todosLosPermisoIds.Except(permisosYaAsignados).ToList();
        if (faltantes.Count > 0)
        {
            foreach (var permisoId in faltantes)
                db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisoId });
            await db.SaveChangesAsync(cancellationToken);
        }

        // --- Rol Gestor de usuarios ---
        var rolGestor = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Gestor de usuarios", cancellationToken);
        if (rolGestor is null)
        {
            rolGestor = new Rol { Nombre = "Gestor de usuarios" };
            db.Roles.Add(rolGestor);
            await db.SaveChangesAsync(cancellationToken);
        }
        if (!await db.RolPermisos.AnyAsync(rp => rp.RolId == rolGestor.Id && rp.PermisoId == permisoGU.Id, cancellationToken))
        {
            db.RolPermisos.Add(new RolPermiso { RolId = rolGestor.Id, PermisoId = permisoGU.Id });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
