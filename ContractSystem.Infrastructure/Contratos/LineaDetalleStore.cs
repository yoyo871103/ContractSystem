using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class LineaDetalleStore : ILineaDetalleStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public LineaDetalleStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<LineaDetalle>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<LineaDetalle>();

        return await db.Set<LineaDetalle>()
            .Where(e => e.ContratoId == contratoId)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LineaDetalle>> GetByAnexoAsync(int anexoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<LineaDetalle>();

        return await db.Set<LineaDetalle>()
            .Where(e => e.AnexoId == anexoId)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<LineaDetalle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<LineaDetalle>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<LineaDetalle> CreateAsync(LineaDetalle linea, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<LineaDetalle>().Add(linea);
        await db.SaveChangesAsync(cancellationToken);
        return linea;
    }

    public async Task UpdateAsync(LineaDetalle linea, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<LineaDetalle>().Update(linea);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<LineaDetalle>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return;

        db.Set<LineaDetalle>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
