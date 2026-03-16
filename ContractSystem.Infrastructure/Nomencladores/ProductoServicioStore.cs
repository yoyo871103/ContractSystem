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
