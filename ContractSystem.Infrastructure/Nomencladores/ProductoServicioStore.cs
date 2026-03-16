using ContractSystem.Application.Common.Models;
using ContractSystem.Application.Database;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Nomencladores;

public sealed class ProductoServicioStore : IProductoServicioStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public ProductoServicioStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ProductoServicio?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<ProductoServicio>()
            .Include(e => e.UnidadMedida)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductoServicio>> GetAllAsync(bool includeDeleted = false, TipoProductoServicio? tipo = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<ProductoServicio>();

        var query = db.Set<ProductoServicio>().Include(e => e.UnidadMedida).AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.Tipo == tipo.Value);

        return await query
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<ProductoServicio>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false, TipoProductoServicio? tipo = null, string? searchText = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return PagedList<ProductoServicio>.Empty(pageSize);

        var query = db.Set<ProductoServicio>().Include(e => e.UnidadMedida).AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.Tipo == tipo.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(e =>
                e.Nombre.ToLower().Contains(term) ||
                (e.Codigo != null && e.Codigo.ToLower().Contains(term)));
        }

        var totalRows = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalRows / pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var items = await query
            .OrderBy(e => e.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<ProductoServicio>(items, page, totalPages, pageSize, totalRows, items.Count);
    }

    public async Task<ProductoServicio> CreateAsync(ProductoServicio entity, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<ProductoServicio>().Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(ProductoServicio entity, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<ProductoServicio>().Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<ProductoServicio>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UndeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<ProductoServicio>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }
}
