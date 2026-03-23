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

        await EnsurePermisosYRolesAsync(db, cancellationToken);
    }

    private async Task SeedInitialAdminAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        var (hash, salt) = _passwordHasher.HashPassword(DefaultAdminPassword);

        // Crear todos los permisos
        await EnsureAllPermisosAsync(db, cancellationToken);

        // Crear roles base
        var rolAdmin = new Rol { Nombre = DefaultUsers.RolAdministradorSistema, Descripcion = "Acceso total al sistema" };
        var rolGestorUsuarios = new Rol { Nombre = "Gestor de usuarios", Descripcion = "Puede gestionar usuarios" };
        db.Roles.Add(rolAdmin);
        db.Roles.Add(rolGestorUsuarios);
        await db.SaveChangesAsync(cancellationToken);

        // Asignar TODOS los permisos al rol Admin
        var todosPermisoIds = await db.Permisos.Select(p => p.Id).ToListAsync(cancellationToken);
        foreach (var permisoId in todosPermisoIds)
            db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisoId });

        // Asignar GestionarUsuarios al rol Gestor
        var permisoGU = await db.Permisos.FirstAsync(p => p.Nombre == Permissions.GestionarUsuarios, cancellationToken);
        db.RolPermisos.Add(new RolPermiso { RolId = rolGestorUsuarios.Id, PermisoId = permisoGU.Id });
        await db.SaveChangesAsync(cancellationToken);

        // Crear usuario admin
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
    /// Asegura que todos los permisos definidos en Permissions.Todos existan en la BD,
    /// y que el rol Admin tenga todos los permisos.
    /// </summary>
    private static async Task EnsurePermisosYRolesAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        // Asegurar todos los permisos
        await EnsureAllPermisosAsync(db, cancellationToken);

        // Asegurar rol Administrador de sistema
        var rolAdmin = await db.Roles.FirstOrDefaultAsync(
            r => r.Nombre == DefaultUsers.RolAdministradorSistema || r.Nombre == "Administrador", cancellationToken);
        if (rolAdmin is null)
        {
            rolAdmin = new Rol { Nombre = DefaultUsers.RolAdministradorSistema, Descripcion = "Acceso total al sistema" };
            db.Roles.Add(rolAdmin);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (rolAdmin.Nombre != DefaultUsers.RolAdministradorSistema)
        {
            rolAdmin.Nombre = DefaultUsers.RolAdministradorSistema;
            await db.SaveChangesAsync(cancellationToken);
        }

        // Asignar TODOS los permisos al rol Admin
        var todosPermisoIds = await db.Permisos.Select(p => p.Id).ToListAsync(cancellationToken);
        var permisosYaAsignados = await db.RolPermisos
            .Where(rp => rp.RolId == rolAdmin.Id)
            .Select(rp => rp.PermisoId)
            .ToListAsync(cancellationToken);
        var faltantes = todosPermisoIds.Except(permisosYaAsignados).ToList();
        if (faltantes.Count > 0)
        {
            foreach (var permisoId in faltantes)
                db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisoId });
            await db.SaveChangesAsync(cancellationToken);
        }

        // Asegurar rol Gestor de usuarios
        var rolGestor = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Gestor de usuarios", cancellationToken);
        if (rolGestor is null)
        {
            rolGestor = new Rol { Nombre = "Gestor de usuarios", Descripcion = "Puede gestionar usuarios" };
            db.Roles.Add(rolGestor);
            await db.SaveChangesAsync(cancellationToken);
        }
        var permisoGU = await db.Permisos.FirstOrDefaultAsync(p => p.Nombre == Permissions.GestionarUsuarios, cancellationToken);
        if (permisoGU is not null && !await db.RolPermisos.AnyAsync(rp => rp.RolId == rolGestor.Id && rp.PermisoId == permisoGU.Id, cancellationToken))
        {
            db.RolPermisos.Add(new RolPermiso { RolId = rolGestor.Id, PermisoId = permisoGU.Id });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Crea todos los permisos que no existan aún en la BD.
    /// Actualiza descripción y categoría de los existentes si han cambiado.
    /// </summary>
    private static async Task EnsureAllPermisosAsync(DatabaseApplicationDbContext db, CancellationToken cancellationToken)
    {
        var existentes = await db.Permisos.ToListAsync(cancellationToken);
        var existentesPorNombre = existentes.ToDictionary(p => p.Nombre, StringComparer.OrdinalIgnoreCase);

        foreach (var def in Permissions.Todos)
        {
            if (existentesPorNombre.TryGetValue(def.Nombre, out var permiso))
            {
                var changed = false;
                if (permiso.Descripcion != def.Descripcion)
                {
                    permiso.Descripcion = def.Descripcion;
                    changed = true;
                }
                if (permiso.Categoria != def.Categoria)
                {
                    permiso.Categoria = def.Categoria;
                    changed = true;
                }
                if (changed)
                    db.Permisos.Update(permiso);
            }
            else
            {
                db.Permisos.Add(new Permiso
                {
                    Nombre = def.Nombre,
                    Descripcion = def.Descripcion,
                    Categoria = def.Categoria
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
