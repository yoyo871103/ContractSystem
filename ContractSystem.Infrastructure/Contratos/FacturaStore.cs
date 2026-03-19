using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class FacturaStore : IFacturaStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public FacturaStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<Factura>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Factura>();

        return await db.Set<Factura>()
            .Where(f => f.ContratoId == contratoId)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<Factura?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<Factura>()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<Factura>().Add(factura);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        var entity = await db.Set<Factura>().FirstOrDefaultAsync(f => f.Id == factura.Id, cancellationToken);
        if (entity is null) throw new InvalidOperationException("La factura no existe.");

        entity.Numero = factura.Numero;
        entity.Fecha = factura.Fecha;
        entity.ImporteTotal = factura.ImporteTotal;
        entity.Descripcion = factura.Descripcion;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<Factura>().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null) return;

        db.Set<Factura>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
