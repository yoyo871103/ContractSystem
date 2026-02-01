using InventorySystem.Application.Auth;
using InventorySystem.Application.Database;
using InventorySystem.Application.Common.Models;
using InventorySystem.Domain.Identity;
using InventorySystem.Infrastructure.Pagination;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = InventorySystem.Infrastructure.Database.ApplicationDbContext;

namespace InventorySystem.Infrastructure.Auth;

internal sealed class UsuarioStore : IUsuarioStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public UsuarioStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .ThenInclude(r => r.RolPermisos)
            .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo, cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, cancellationToken);
    }

    public async Task UpdateUltimoAccesoAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (user is null)
            return;

        user.UltimoAcceso = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePasswordAsync(int usuarioId, string hash, string salt, bool requiereCambioContraseña, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (user is null)
            return;

        user.HashContraseña = hash;
        user.Salt = salt;
        user.RequiereCambioContraseña = requiereCambioContraseña;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<UsuarioListItem>> ListPagedAsync(FilterableQuery query, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return PagedList<UsuarioListItem>.Empty(query.RowsPerPage);

        var includeDeleted = (query as UsuariosListQuery)?.IncludeDeleted ?? false;
        var baseQuery = includeDeleted ? db.Usuarios.IgnoreQueryFilters() : db.Usuarios.AsQueryable();

        var q = baseQuery
            .AsNoTracking()
            .Select(u => new UsuarioListItem
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                NombreParaMostrar = u.NombreParaMostrar,
                Activo = u.Activo,
                RequiereCambioContraseña = u.RequiereCambioContraseña,
                RolesDisplay = "",
                IsDeleted = u.DeletedAt != null
            });

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            q = q.Where(u => u.NombreUsuario.Contains(search) || (u.NombreParaMostrar != null && u.NombreParaMostrar.Contains(search)));
        }

        q = ApplySort(q, query.SortBy);

        var result = await q.ToPagedListAsync(query, cancellationToken);
        if (result.Items.Count == 0)
            return result;

        var ids = result.Items.Select(i => i.Id).ToList();
        var usuarioRoles = await db.UsuarioRoles
            .AsNoTracking()
            .Where(ur => ids.Contains(ur.UsuarioId))
            .Include(ur => ur.Rol)
            .ToListAsync(cancellationToken);
        var rolesByUsuario = usuarioRoles
            .GroupBy(ur => ur.UsuarioId)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(ur => ur.Rol.Nombre)));
        var adminUserIds = usuarioRoles
            .Where(ur => string.Equals(ur.Rol.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase))
            .Select(ur => ur.UsuarioId)
            .ToHashSet();

        var itemsWithRoles = result.Items.Select(item => item with
        {
            RolesDisplay = rolesByUsuario.TryGetValue(item.Id, out var names) ? names : "",
            IsAdministrador = adminUserIds.Contains(item.Id),
            IsDefaultAdmin = string.Equals(item.NombreUsuario, DefaultUsers.NombreUsuarioAdmin, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        return new PagedList<UsuarioListItem>(itemsWithRoles, result.CurrentPage, result.TotalPages,
            result.TotalRowsPerPage, result.TotalRows, result.SelectedPageSize);
    }

    private static IQueryable<UsuarioListItem> ApplySort(IQueryable<UsuarioListItem> q, string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return q.OrderBy(u => u.NombreUsuario);

        return sortBy.Trim().ToUpperInvariant() switch
        {
            "NOMBREPARAMOSTRAR" or "NOMBRE" => q.OrderBy(u => u.NombreParaMostrar),
            "ACTIVO" => q.OrderBy(u => u.Activo),
            "REQUIERECAMBIOCONTRASEÑA" => q.OrderBy(u => u.RequiereCambioContraseña),
            _ => q.OrderBy(u => u.NombreUsuario)
        };
    }

    public async Task UpdateProfileAsync(int usuarioId, string nombreParaMostrar, string? email, byte[]? fotoPerfil, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (user is null)
            return;

        user.NombreParaMostrar = nombreParaMostrar;
        user.Email = email;
        user.FotoPerfil = fotoPerfil;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDeletedAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        if (user is null)
            return;

        if (string.Equals(user.NombreUsuario, DefaultUsers.NombreUsuarioAdmin, StringComparison.OrdinalIgnoreCase))
            return;

        user.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetUndeletedAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        if (user is null)
            return;

        user.DeletedAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Usuario?> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        var exists = await db.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario.Trim(), cancellationToken);
        if (exists)
            return null;

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario.Trim(),
            NombreParaMostrar = request.NombreParaMostrar.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            HashContraseña = request.HashContraseña,
            Salt = request.Salt,
            RequiereCambioContraseña = request.RequiereCambioContraseña,
            Activo = true,
            FechaCreacion = DateTimeOffset.UtcNow
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(cancellationToken);

        var rolIds = request.RolIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
        if (rolIds.Count > 0)
        {
            foreach (var rolId in rolIds)
                db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rolId });
            await db.SaveChangesAsync(cancellationToken);
        }

        return usuario;
    }

    public async Task<UsuarioEditDto?> GetByIdForEditAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        var u = await db.Usuarios
            .AsNoTracking()
            .Include(us => us.UsuarioRoles)
            .FirstOrDefaultAsync(us => us.Id == usuarioId, cancellationToken);
        if (u is null)
            return null;

        return new UsuarioEditDto
        {
            Id = u.Id,
            NombreUsuario = u.NombreUsuario,
            NombreParaMostrar = u.NombreParaMostrar,
            Email = u.Email,
            Activo = u.Activo,
            RolIds = u.UsuarioRoles.Select(ur => ur.RolId).ToList()
        };
    }

    public async Task UpdateUsuarioAsync(int usuarioId, string nombreParaMostrar, string? email, bool activo, IReadOnlyList<int> rolIds, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var user = await db.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        if (user is null)
            return;

        user.NombreParaMostrar = nombreParaMostrar.Trim();
        user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        user.Activo = string.Equals(user.NombreUsuario, DefaultUsers.NombreUsuarioAdmin, StringComparison.OrdinalIgnoreCase)
            ? true
            : activo;

        var newRolIds = (rolIds ?? Array.Empty<int>()).Where(id => id > 0).Distinct().ToHashSet();
        var current = user.UsuarioRoles.Select(ur => ur.RolId).ToHashSet();
        foreach (var ur in user.UsuarioRoles.Where(ur => !newRolIds.Contains(ur.RolId)).ToList())
            db.UsuarioRoles.Remove(ur);
        foreach (var rolId in newRolIds.Where(id => !current.Contains(id)))
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioId, RolId = rolId });

        await db.SaveChangesAsync(cancellationToken);
    }
}
