using ContractSystem.Application.Auth;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Auth;

internal sealed class RolStore : IRolStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public RolStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<RolItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<RolItem>();

        return await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new RolItem(r.Id, r.Nombre))
            .ToListAsync(cancellationToken);
    }

    public async Task<RolDetailDto?> GetByIdAsync(int rolId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        var rol = await db.Roles
            .AsNoTracking()
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol is null)
            return null;

        return new RolDetailDto(
            rol.Id,
            rol.Nombre,
            rol.Descripcion,
            rol.RolPermisos.Select(rp => rp.PermisoId).ToList());
    }

    public async Task<IReadOnlyList<RolListItem>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<RolListItem>();

        return await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new RolListItem(
                r.Id,
                r.Nombre,
                r.Descripcion,
                r.RolPermisos.Count,
                r.UsuarioRoles.Count,
                r.Nombre == DefaultUsers.RolAdministradorSistema))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PermisoItem>> GetAllPermisosAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<PermisoItem>();

        return await db.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Categoria).ThenBy(p => p.Nombre)
            .Select(p => new PermisoItem(p.Id, p.Nombre, p.Descripcion, p.Categoria))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(string nombre, string? descripcion, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        var rol = new Rol
        {
            Nombre = nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()
        };
        db.Roles.Add(rol);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var permisoId in permisoIds.Distinct())
            db.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
        await db.SaveChangesAsync(cancellationToken);

        return rol.Id;
    }

    public async Task UpdateAsync(int rolId, string nombre, string? descripcion, IReadOnlyList<int> permisoIds, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var rol = await db.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol is null)
            return;

        // No permitir renombrar el rol Admin
        if (rol.Nombre != DefaultUsers.RolAdministradorSistema)
        {
            rol.Nombre = nombre.Trim();
            rol.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        }

        // Para el rol Admin, siempre debe tener TODOS los permisos - no permitir quitar
        if (rol.Nombre == DefaultUsers.RolAdministradorSistema)
            return;

        // Sincronizar permisos
        var newPermisoIds = permisoIds.Distinct().ToHashSet();
        var current = rol.RolPermisos.Select(rp => rp.PermisoId).ToHashSet();

        foreach (var rp in rol.RolPermisos.Where(rp => !newPermisoIds.Contains(rp.PermisoId)).ToList())
            db.RolPermisos.Remove(rp);
        foreach (var permisoId in newPermisoIds.Where(id => !current.Contains(id)))
            db.RolPermisos.Add(new RolPermiso { RolId = rolId, PermisoId = permisoId });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int rolId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == rolId, cancellationToken);
        if (rol is null)
            return;

        // No permitir eliminar rol de sistema
        if (rol.Nombre == DefaultUsers.RolAdministradorSistema)
            return;

        db.Roles.Remove(rol);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return false;

        var query = db.Roles.Where(r => r.Nombre == nombre.Trim());
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}
