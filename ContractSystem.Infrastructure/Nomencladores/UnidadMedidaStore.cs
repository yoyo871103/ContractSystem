using ContractSystem.Application.Database;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Nomencladores;

public sealed class UnidadMedidaStore : IUnidadMedidaStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public UnidadMedidaStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<UnidadMedida?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<UnidadMedida>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<UnidadMedida>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<UnidadMedida>();

        var query = db.Set<UnidadMedida>().AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        return await query
            .OrderBy(e => e.NombreCorto)
            .ToListAsync(cancellationToken);
    }

    public async Task<UnidadMedida> CreateAsync(UnidadMedida unidadMedida, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<UnidadMedida>().Add(unidadMedida);
        await db.SaveChangesAsync(cancellationToken);
        return unidadMedida;
    }

    public async Task UpdateAsync(UnidadMedida unidadMedida, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<UnidadMedida>().Update(unidadMedida);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<UnidadMedida>()
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

        var entity = await db.Set<UnidadMedida>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }
}
