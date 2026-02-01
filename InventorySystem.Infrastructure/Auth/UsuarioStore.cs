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

        var q = db.Usuarios
            .AsNoTracking()
            .Select(u => new UsuarioListItem
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                NombreParaMostrar = u.NombreParaMostrar,
                Activo = u.Activo,
                RequiereCambioContraseña = u.RequiereCambioContraseña
            });

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            q = q.Where(u => u.NombreUsuario.Contains(search) || (u.NombreParaMostrar != null && u.NombreParaMostrar.Contains(search)));
        }

        q = ApplySort(q, query.SortBy);

        return await q.ToPagedListAsync(query, cancellationToken);
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

        user.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
